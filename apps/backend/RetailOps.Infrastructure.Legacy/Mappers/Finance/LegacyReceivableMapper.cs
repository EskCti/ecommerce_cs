using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.Sales;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Finance;

internal enum LegacyReceivableType
{
    Venda,
    Empresa,
    Other
}

internal static class LegacyReceivableMapper
{
    public const string ManualType = "Conta";
    public const string AttachmentType = "Receber";

    internal static LegacyReceivableType ResolveType(string? legacyType)
    {
        if (string.IsNullOrWhiteSpace(legacyType))
            return LegacyReceivableType.Other;

        if (legacyType.StartsWith("Venda", StringComparison.OrdinalIgnoreCase))
            return LegacyReceivableType.Venda;

        if (legacyType.Equals("Empresa", StringComparison.OrdinalIgnoreCase))
            return LegacyReceivableType.Empresa;

        return LegacyReceivableType.Other;
    }

    public static Result<Receivable> ToDomain(
        LegacyReceivableRow row,
        IEnumerable<FinanceAttachment>? attachments = null)
    {
        if (ResolveType(row.Type) != LegacyReceivableType.Other)
            return Result<Receivable>.Failure("Receivable type is not manual.");

        var tenantResult = TenantId.Create(row.CompanyId);
        if (tenantResult.IsFailure)
            return Result<Receivable>.Failure(tenantResult.Error);

        var amountResult = Money.Create(row.Amount);
        if (amountResult.IsFailure)
            return Result<Receivable>.Failure(amountResult.Error);

        var dueDateResult = DueDate.Create(row.DueDate);
        if (dueDateResult.IsFailure)
            return Result<Receivable>.Failure(dueDateResult.Error);

        var status = IsPaid(row.Paid) ? PaymentStatus.Settled : PaymentStatus.Open;
        var settledAt = status == PaymentStatus.Settled ? row.CompletedAt ?? row.DueDate : (DateTime?)null;
        var description = string.IsNullOrWhiteSpace(row.Description) ? row.Type : row.Description;

        return Receivable.Reconstitute(
            LegacyFinanceIds.Receivable(row.Id),
            tenantResult.Value,
            description!,
            amountResult.Value,
            dueDateResult.Value,
            status,
            settledAt,
            saleId: null,
            row.PersonId > 0 ? row.PersonId : null,
            row.DueDate,
            row.DueDate,
            attachments);
    }

    public static Result<Receivable> ToSaleLinkedDomain(LegacyReceivableRow row)
    {
        if (ResolveType(row.Type) != LegacyReceivableType.Venda)
            return Result<Receivable>.Failure("Receivable is not sale-linked.");

        var tenantResult = TenantId.Create(row.CompanyId);
        if (tenantResult.IsFailure)
            return Result<Receivable>.Failure(tenantResult.Error);

        var amountResult = Money.Create(row.Amount);
        if (amountResult.IsFailure)
            return Result<Receivable>.Failure(amountResult.Error);

        var dueDateResult = DueDate.Create(row.DueDate);
        if (dueDateResult.IsFailure)
            return Result<Receivable>.Failure(dueDateResult.Error);

        var status = IsPaid(row.Paid) ? PaymentStatus.Settled : PaymentStatus.Open;
        var settledAt = status == PaymentStatus.Settled ? row.CompletedAt ?? row.DueDate : (DateTime?)null;

        return Receivable.Reconstitute(
            LegacySalesIds.Sale(row.Id),
            tenantResult.Value,
            row.Type,
            amountResult.Value,
            dueDateResult.Value,
            status,
            settledAt,
            LegacySalesIds.Sale(row.Id),
            row.PersonId > 0 ? row.PersonId : null,
            row.CompletedAt ?? row.DueDate,
            row.CompletedAt ?? row.DueDate);
    }

    public static LegacyReceivableRow ToRow(Receivable receivable, int? legacyId = null)
    {
        return new LegacyReceivableRow
        {
            Id = legacyId ?? LegacyFinanceIds.ParseReceivableLegacyId(receivable.Id) ?? 0,
            CompanyId = receivable.TenantId.Value,
            Type = ManualType,
            Description = receivable.Description,
            PersonId = receivable.PersonLegacyId ?? 0,
            Amount = receivable.Amount.Amount,
            DueDate = receivable.DueDate.Value,
            Paid = receivable.Status == PaymentStatus.Settled ? "Sim" : "Não",
            CompletedAt = receivable.SettledAt
        };
    }

    public static bool IsPaid(string? paid) =>
        paid is not null
        && (paid.Equals("S", StringComparison.OrdinalIgnoreCase)
            || paid.Equals("Sim", StringComparison.OrdinalIgnoreCase));
}
