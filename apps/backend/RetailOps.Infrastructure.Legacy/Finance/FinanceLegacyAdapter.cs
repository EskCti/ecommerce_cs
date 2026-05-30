using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Finance.Application.Ports;
using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.Finance;
using RetailOps.Infrastructure.Legacy.Mappers.Sales;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Finance;

public sealed class FinanceLegacyAdapter(LegacySasDbContext db) : IFinanceLegacyPort
{
    private static readonly string[] ManualReceivableTypes = ["Conta", "Outro", "Receita"];

    public async Task<Result<IReadOnlyList<Receivable>>> GetManualReceivablesFromLegacyAsync(
        TenantId tenantId,
        PaymentStatus? status,
        DateTime? dueFrom,
        DateTime? dueTo,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Receivables.AsNoTracking()
            .Where(r => r.CompanyId == tenantId.Value)
            .Where(r => ManualReceivableTypes.Contains(r.Type)
                || (!r.Type.StartsWith("Venda") && !r.Type.Equals("Empresa")));

        query = ApplyReceivableFilters(query, status, dueFrom, dueTo);

        var rows = await query
            .OrderByDescending(r => r.DueDate)
            .Skip(Math.Max(0, page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return await MapReceivablesAsync(tenantId, rows);
    }

    public async Task<Result<int>> CountManualReceivablesFromLegacyAsync(
        TenantId tenantId,
        PaymentStatus? status,
        DateTime? dueFrom,
        DateTime? dueTo,
        CancellationToken ct = default)
    {
        var query = db.Receivables.AsNoTracking()
            .Where(r => r.CompanyId == tenantId.Value)
            .Where(r => ManualReceivableTypes.Contains(r.Type)
                || (!r.Type.StartsWith("Venda") && !r.Type.Equals("Empresa")));

        query = ApplyReceivableFilters(query, status, dueFrom, dueTo);
        var count = await query.CountAsync(ct);
        return Result<int>.Success(count);
    }

    public async Task<Result> SaveManualReceivableToLegacyAsync(Receivable receivable, CancellationToken ct = default)
    {
        if (receivable.SaleId is not null)
            return Result.Failure("Use Sales ACL for sale-linked receivables.");

        var legacyId = LegacyFinanceIds.ParseReceivableLegacyId(receivable.Id);
        LegacyReceivableRow row;

        if (legacyId is null or <= 0)
        {
            row = LegacyReceivableMapper.ToRow(receivable);
            db.Receivables.Add(row);
        }
        else
        {
            row = await db.Receivables.FirstAsync(r => r.Id == legacyId, ct);
            var mapped = LegacyReceivableMapper.ToRow(receivable, legacyId);
            row.Type = mapped.Type;
            row.Description = mapped.Description;
            row.PersonId = mapped.PersonId;
            row.Amount = mapped.Amount;
            row.DueDate = mapped.DueDate;
            row.Paid = mapped.Paid;
            row.CompletedAt = mapped.CompletedAt;
        }

        await db.SaveChangesAsync(ct);
        receivable.SyncIdentity(LegacyFinanceIds.Receivable(row.Id));
        return Result.Success();
    }

    public async Task<Result> DeleteManualReceivableFromLegacyAsync(
        TenantId tenantId,
        Guid receivableId,
        CancellationToken ct = default)
    {
        var legacyId = LegacyFinanceIds.ParseReceivableLegacyId(receivableId);
        if (legacyId is null)
            return Result.Failure("Invalid receivable id.");

        var row = await db.Receivables.FirstOrDefaultAsync(
            r => r.Id == legacyId && r.CompanyId == tenantId.Value,
            ct);

        if (row is null)
            return Result.Failure("Receivable not found.");

        db.Receivables.Remove(row);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<Payable>>> GetPayablesFromLegacyAsync(
        TenantId tenantId,
        AccountType? type,
        PaymentStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Payables.AsNoTracking().Where(p => p.CompanyId == tenantId.Value);
        query = ApplyPayableFilters(query, type, status);

        var rows = await query
            .OrderByDescending(p => p.DueDate)
            .Skip(Math.Max(0, page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return await MapPayablesAsync(tenantId, rows);
    }

    public async Task<Result<int>> CountPayablesFromLegacyAsync(
        TenantId tenantId,
        AccountType? type,
        PaymentStatus? status,
        CancellationToken ct = default)
    {
        var query = db.Payables.AsNoTracking().Where(p => p.CompanyId == tenantId.Value);
        query = ApplyPayableFilters(query, type, status);
        return Result<int>.Success(await query.CountAsync(ct));
    }

    public async Task<Result> SavePayableToLegacyAsync(Payable payable, CancellationToken ct = default)
    {
        var legacyId = LegacyFinanceIds.ParsePayableLegacyId(payable.Id);
        LegacyPayableRow row;

        if (legacyId is null or <= 0)
        {
            row = LegacyPayableMapper.ToRow(payable);
            db.Payables.Add(row);
        }
        else
        {
            row = await db.Payables.FirstAsync(p => p.Id == legacyId, ct);
            var mapped = LegacyPayableMapper.ToRow(payable, legacyId);
            row.Type = mapped.Type;
            row.Description = mapped.Description;
            row.PersonId = mapped.PersonId;
            row.Amount = mapped.Amount;
            row.DueDate = mapped.DueDate;
            row.Paid = mapped.Paid;
            row.FrequencyDays = mapped.FrequencyDays;
            row.ReferenceId = mapped.ReferenceId;
            row.SettledAt = mapped.SettledAt;
        }

        await db.SaveChangesAsync(ct);
        payable.SyncIdentity(LegacyFinanceIds.Payable(row.Id));
        return Result.Success();
    }

    public async Task<Result> DeletePayableFromLegacyAsync(
        TenantId tenantId,
        Guid payableId,
        CancellationToken ct = default)
    {
        var legacyId = LegacyFinanceIds.ParsePayableLegacyId(payableId);
        if (legacyId is null)
            return Result.Failure("Invalid payable id.");

        var row = await db.Payables.FirstOrDefaultAsync(
            p => p.Id == legacyId && p.CompanyId == tenantId.Value,
            ct);

        if (row is null)
            return Result.Failure("Payable not found.");

        db.Payables.Remove(row);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<Commission>>> GetCommissionsFromLegacyAsync(
        TenantId tenantId,
        bool? isPaid,
        int? sellerLegacyId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Commissions.AsNoTracking().Where(c => c.CompanyId == tenantId.Value);

        if (isPaid is true)
            query = query.Where(c => c.Paid == "Sim");
        else if (isPaid is false)
            query = query.Where(c => c.Paid != "Sim");

        if (sellerLegacyId is > 0)
            query = query.Where(c => c.SellerLegacyId == sellerLegacyId);

        var rows = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip(Math.Max(0, page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = new List<Commission>();
        foreach (var row in rows)
        {
            var mapped = LegacyCommissionMapper.ToDomain(row);
            if (mapped.IsFailure)
                return Result<IReadOnlyList<Commission>>.Failure(mapped.Error);
            items.Add(mapped.Value);
        }

        return Result<IReadOnlyList<Commission>>.Success(items);
    }

    public async Task<Result<int>> CountCommissionsFromLegacyAsync(
        TenantId tenantId,
        bool? isPaid,
        int? sellerLegacyId,
        CancellationToken ct = default)
    {
        var query = db.Commissions.AsNoTracking().Where(c => c.CompanyId == tenantId.Value);

        if (isPaid is true)
            query = query.Where(c => c.Paid == "Sim");
        else if (isPaid is false)
            query = query.Where(c => c.Paid != "Sim");

        if (sellerLegacyId is > 0)
            query = query.Where(c => c.SellerLegacyId == sellerLegacyId);

        return Result<int>.Success(await query.CountAsync(ct));
    }

    public async Task<Result> SaveCommissionToLegacyAsync(Commission commission, CancellationToken ct = default)
    {
        var legacyId = LegacyFinanceIds.ParseCommissionLegacyId(commission.Id);
        LegacyCommissionRow row;

        if (legacyId is null or <= 0)
        {
            row = LegacyCommissionMapper.ToRow(commission);
            db.Commissions.Add(row);
        }
        else
        {
            row = await db.Commissions.FirstAsync(c => c.Id == legacyId, ct);
            var mapped = LegacyCommissionMapper.ToRow(commission, legacyId);
            row.Amount = mapped.Amount;
            row.SellerLegacyId = mapped.SellerLegacyId;
            row.SaleLegacyId = mapped.SaleLegacyId;
            row.Paid = mapped.Paid;
            row.PaidAt = mapped.PaidAt;
            row.PaymentPayableLegacyId = mapped.PaymentPayableLegacyId;
        }

        await db.SaveChangesAsync(ct);
        commission.SyncIdentity(LegacyFinanceIds.Commission(row.Id));
        return Result.Success();
    }

    public async Task<Result> DeleteCommissionBySaleFromLegacyAsync(
        TenantId tenantId,
        Guid saleId,
        CancellationToken ct = default)
    {
        var saleLegacyId = LegacySalesIds.ParseSaleLegacyId(saleId);
        if (saleLegacyId is null)
            return Result.Failure("Invalid sale id.");

        var rows = await db.Commissions
            .Where(c => c.CompanyId == tenantId.Value && c.SaleLegacyId == saleLegacyId)
            .ToListAsync(ct);

        if (rows.Count == 0)
            return Result.Success();

        db.Commissions.RemoveRange(rows);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<FinanceAttachment>>> GetReceivableAttachmentsFromLegacyAsync(
        TenantId tenantId,
        Guid receivableId,
        CancellationToken ct = default)
        => await GetAttachmentsAsync(tenantId, LegacyReceivableMapper.AttachmentType, receivableId, ct);

    public async Task<Result<IReadOnlyList<FinanceAttachment>>> GetPayableAttachmentsFromLegacyAsync(
        TenantId tenantId,
        Guid payableId,
        CancellationToken ct = default)
        => await GetAttachmentsAsync(tenantId, LegacyPayableMapper.AttachmentType, payableId, ct);

    public async Task<Result> SaveReceivableAttachmentToLegacyAsync(
        Receivable receivable,
        FinanceAttachment attachment,
        CancellationToken ct = default)
        => await SaveAttachmentAsync(receivable.TenantId, LegacyReceivableMapper.AttachmentType, receivable.Id, attachment, ct);

    public async Task<Result> SavePayableAttachmentToLegacyAsync(
        Payable payable,
        FinanceAttachment attachment,
        CancellationToken ct = default)
        => await SaveAttachmentAsync(payable.TenantId, LegacyPayableMapper.AttachmentType, payable.Id, attachment, ct);

    public async Task<Result<CashFlowSummary>> GetCashFlowFromLegacyAsync(
        TenantId tenantId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var receivables = await db.Receivables.AsNoTracking()
            .Where(r => r.CompanyId == tenantId.Value)
            .Where(r => r.Paid == "Sim")
            .Where(r => (r.CompletedAt ?? r.DueDate) >= from && (r.CompletedAt ?? r.DueDate) <= to)
            .ToListAsync(ct);

        var payables = await db.Payables.AsNoTracking()
            .Where(p => p.CompanyId == tenantId.Value)
            .Where(p => p.Paid == "Sim")
            .Where(p => (p.SettledAt ?? p.DueDate) >= from && (p.SettledAt ?? p.DueDate) <= to)
            .ToListAsync(ct);

        var lines = new List<CashFlowLine>();
        decimal inflow = 0;
        decimal outflow = 0;

        foreach (var row in receivables)
        {
            inflow += row.Amount;
            lines.Add(new CashFlowLine("Inflow", row.Type, row.Amount, row.CompletedAt ?? row.DueDate));
        }

        foreach (var row in payables)
        {
            outflow += row.Amount;
            lines.Add(new CashFlowLine("Outflow", row.Description, row.Amount, row.SettledAt ?? row.DueDate));
        }

        return Result<CashFlowSummary>.Success(new CashFlowSummary(
            inflow,
            outflow,
            inflow - outflow,
            lines.OrderBy(l => l.Date).ToList()));
    }

    internal async Task<Result<Receivable>> GetReceivableByIdAsync(Guid id, CancellationToken ct = default)
    {
        var manualLegacyId = LegacyFinanceIds.ParseReceivableLegacyId(id);
        if (manualLegacyId is > 0)
        {
            var row = await db.Receivables.AsNoTracking().FirstOrDefaultAsync(r => r.Id == manualLegacyId, ct);
            if (row is null)
                return Result<Receivable>.Failure("Receivable not found.");

            var attachments = await LoadAttachmentsAsync(row.CompanyId, LegacyReceivableMapper.AttachmentType, id, ct);
            if (attachments.IsFailure)
                return Result<Receivable>.Failure(attachments.Error);

            return LegacyReceivableMapper.ToDomain(row, attachments.Value);
        }

        var saleLegacyId = LegacySalesIds.ParseSaleLegacyId(id);
        if (saleLegacyId is > 0)
        {
            var row = await db.Receivables.AsNoTracking().FirstOrDefaultAsync(r => r.Id == saleLegacyId, ct);
            if (row is null)
                return Result<Receivable>.Failure("Receivable not found.");

            return LegacyReceivableMapper.ToSaleLinkedDomain(row);
        }

        return Result<Receivable>.Failure("Invalid receivable id.");
    }

    internal async Task<Result<Receivable?>> GetReceivableBySaleIdAsync(Guid saleId, CancellationToken ct = default)
    {
        var saleLegacyId = LegacySalesIds.ParseSaleLegacyId(saleId);
        if (saleLegacyId is null)
            return Result<Receivable?>.Failure("Invalid sale id.");

        var row = await db.Receivables.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == saleLegacyId, ct);

        if (row is null)
            return Result<Receivable?>.Success(null);

        var mapped = LegacyReceivableMapper.ToSaleLinkedDomain(row);
        return mapped.IsFailure
            ? Result<Receivable?>.Failure(mapped.Error)
            : Result<Receivable?>.Success(mapped.Value);
    }

    internal async Task<Result<Payable>> GetPayableByIdAsync(Guid id, CancellationToken ct = default)
    {
        var legacyId = LegacyFinanceIds.ParsePayableLegacyId(id);
        if (legacyId is null)
            return Result<Payable>.Failure("Invalid payable id.");

        var row = await db.Payables.AsNoTracking().FirstOrDefaultAsync(p => p.Id == legacyId, ct);
        if (row is null)
            return Result<Payable>.Failure("Payable not found.");

        var attachments = await LoadAttachmentsAsync(row.CompanyId, LegacyPayableMapper.AttachmentType, id, ct);
        if (attachments.IsFailure)
            return Result<Payable>.Failure(attachments.Error);

        return LegacyPayableMapper.ToDomain(row, attachments.Value);
    }

    internal async Task<Result<Commission>> GetCommissionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var legacyId = LegacyFinanceIds.ParseCommissionLegacyId(id);
        if (legacyId is null)
            return Result<Commission>.Failure("Invalid commission id.");

        var row = await db.Commissions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == legacyId, ct);
        if (row is null)
            return Result<Commission>.Failure("Commission not found.");

        return LegacyCommissionMapper.ToDomain(row);
    }

    internal async Task<Result<Commission?>> GetCommissionBySaleIdAsync(Guid saleId, CancellationToken ct = default)
    {
        var saleLegacyId = LegacySalesIds.ParseSaleLegacyId(saleId);
        if (saleLegacyId is null)
            return Result<Commission?>.Failure("Invalid sale id.");

        var row = await db.Commissions.AsNoTracking()
            .FirstOrDefaultAsync(c => c.SaleLegacyId == saleLegacyId, ct);

        if (row is null)
            return Result<Commission?>.Success(null);

        var mapped = LegacyCommissionMapper.ToDomain(row);
        return mapped.IsFailure
            ? Result<Commission?>.Failure(mapped.Error)
            : Result<Commission?>.Success(mapped.Value);
    }

    private async Task<Result<IReadOnlyList<Receivable>>> MapReceivablesAsync(
        TenantId tenantId,
        IReadOnlyList<LegacyReceivableRow> rows)
    {
        var items = new List<Receivable>();
        foreach (var row in rows)
        {
            var id = LegacyFinanceIds.Receivable(row.Id);
            var attachments = await LoadAttachmentsAsync(tenantId.Value, LegacyReceivableMapper.AttachmentType, id, CancellationToken.None);
            if (attachments.IsFailure)
                return Result<IReadOnlyList<Receivable>>.Failure(attachments.Error);

            var mapped = LegacyReceivableMapper.ToDomain(row, attachments.Value);
            if (mapped.IsFailure)
                return Result<IReadOnlyList<Receivable>>.Failure(mapped.Error);
            items.Add(mapped.Value);
        }

        return Result<IReadOnlyList<Receivable>>.Success(items);
    }

    private async Task<Result<IReadOnlyList<Payable>>> MapPayablesAsync(
        TenantId tenantId,
        IReadOnlyList<LegacyPayableRow> rows)
    {
        var items = new List<Payable>();
        foreach (var row in rows)
        {
            var id = LegacyFinanceIds.Payable(row.Id);
            var attachments = await LoadAttachmentsAsync(tenantId.Value, LegacyPayableMapper.AttachmentType, id, CancellationToken.None);
            if (attachments.IsFailure)
                return Result<IReadOnlyList<Payable>>.Failure(attachments.Error);

            var mapped = LegacyPayableMapper.ToDomain(row, attachments.Value);
            if (mapped.IsFailure)
                return Result<IReadOnlyList<Payable>>.Failure(mapped.Error);
            items.Add(mapped.Value);
        }

        return Result<IReadOnlyList<Payable>>.Success(items);
    }

    private static IQueryable<LegacyReceivableRow> ApplyReceivableFilters(
        IQueryable<LegacyReceivableRow> query,
        PaymentStatus? status,
        DateTime? dueFrom,
        DateTime? dueTo)
    {
        if (status is PaymentStatus.Open)
            query = query.Where(r => r.Paid != "Sim");
        else if (status is PaymentStatus.Settled)
            query = query.Where(r => r.Paid == "Sim");

        if (dueFrom is not null)
            query = query.Where(r => r.DueDate >= dueFrom);

        if (dueTo is not null)
            query = query.Where(r => r.DueDate <= dueTo);

        return query;
    }

    private static IQueryable<LegacyPayableRow> ApplyPayableFilters(
        IQueryable<LegacyPayableRow> query,
        AccountType? type,
        PaymentStatus? status)
    {
        if (type is AccountType.Expense)
            query = query.Where(p => p.Type == "Conta");
        else if (type is AccountType.Purchase)
            query = query.Where(p => p.Type == "Compra");
        else if (type is AccountType.CommissionPayment)
            query = query.Where(p => p.Type == "Pagamento");

        if (status is PaymentStatus.Open)
            query = query.Where(p => p.Paid != "Sim");
        else if (status is PaymentStatus.Settled)
            query = query.Where(p => p.Paid == "Sim");

        return query;
    }

    private async Task<Result<IReadOnlyList<FinanceAttachment>>> LoadAttachmentsAsync(
        int tenantId,
        string attachmentType,
        Guid ownerId,
        CancellationToken ct)
    {
        var legacyOwnerId = LegacyFinanceIds.ParseReceivableLegacyId(ownerId)
            ?? LegacyFinanceIds.ParsePayableLegacyId(ownerId);

        if (legacyOwnerId is null)
            return Result<IReadOnlyList<FinanceAttachment>>.Success([]);

        var tenantResult = TenantId.Create(tenantId);
        if (tenantResult.IsFailure)
            return Result<IReadOnlyList<FinanceAttachment>>.Failure(tenantResult.Error);

        var rows = await db.Attachments.AsNoTracking()
            .Where(a => a.CompanyId == tenantId && a.Tipo == attachmentType && a.IdRef == legacyOwnerId)
            .ToListAsync(ct);

        var items = new List<FinanceAttachment>();
        foreach (var row in rows)
        {
            var metaResult = AttachmentMeta.Create(row.Nome ?? "Anexo", row.Foto ?? string.Empty);
            if (metaResult.IsFailure)
                continue;

            var attachmentResult = FinanceAttachment.Reconstitute(
                Mappers.Crm.LegacyCrmIds.Attachment(row.Id),
                tenantResult.Value,
                ownerId,
                metaResult.Value,
                row.DataValidade ?? DateTime.UtcNow);

            if (attachmentResult.IsSuccess)
                items.Add(attachmentResult.Value);
        }

        return Result<IReadOnlyList<FinanceAttachment>>.Success(items);
    }

    private async Task<Result> SaveAttachmentAsync(
        TenantId tenantId,
        string attachmentType,
        Guid ownerId,
        FinanceAttachment attachment,
        CancellationToken ct)
    {
        var legacyOwnerId = LegacyFinanceIds.ParseReceivableLegacyId(ownerId)
            ?? LegacyFinanceIds.ParsePayableLegacyId(ownerId);

        if (legacyOwnerId is null)
            return Result.Failure("Owner legacy id not found.");

        var row = new LegacyAttachmentRow
        {
            CompanyId = tenantId.Value,
            Tipo = attachmentType,
            IdRef = legacyOwnerId.Value,
            Nome = attachment.Meta.Name,
            Foto = attachment.Meta.Path,
            DataValidade = attachment.CreatedAt
        };

        db.Attachments.Add(row);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result<IReadOnlyList<FinanceAttachment>>> GetAttachmentsAsync(
        TenantId tenantId,
        string attachmentType,
        Guid ownerId,
        CancellationToken ct)
        => await LoadAttachmentsAsync(tenantId.Value, attachmentType, ownerId, ct);
}
