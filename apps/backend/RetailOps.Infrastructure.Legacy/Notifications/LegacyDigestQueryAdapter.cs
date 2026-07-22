using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Notifications.Application.Ports;
using RetailOps.Core.Notifications.Domain.Entities;
using RetailOps.Core.Notifications.Domain.Repositories;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Notifications;

public sealed class LegacyDigestQueryAdapter(LegacySasDbContext db)
    : IReceivablesDueTodayQueryPort,
        ILowStockSummaryQueryPort,
        ITenantBillingAlertQueryPort,
        IActiveTenantIdsQueryPort
{
    public async Task<Result<ReceivablesDueTodaySummary>> GetDueTodaySummaryAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        var companyId = tenantId.Value;
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var rows = await db.Receivables.AsNoTracking()
            .Where(r => r.CompanyId == companyId)
            .Where(r => r.DueDate >= today && r.DueDate < tomorrow)
            .Where(r => r.Paid != "Sim")
            .Where(r => r.Cancelled != "Sim")
            .ToListAsync(ct);

        return Result<ReceivablesDueTodaySummary>.Success(
            new ReceivablesDueTodaySummary(rows.Count, rows.Sum(r => r.Amount)));
    }

    public async Task<Result<LowStockSummary>> GetLowStockSummaryAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        var companyId = tenantId.Value;

        var products = await db.Products.AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .Where(p => p.Active == "Sim" || p.Active == null)
            .Where(p => p.Stock <= p.StockAlertLevel)
            .OrderBy(p => p.Stock)
            .ThenBy(p => p.Name)
            .Take(10)
            .ToListAsync(ct);

        var totalCount = await db.Products.AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .Where(p => p.Active == "Sim" || p.Active == null)
            .Where(p => p.Stock <= p.StockAlertLevel)
            .CountAsync(ct);

        return Result<LowStockSummary>.Success(new LowStockSummary(
            totalCount,
            products.Select(p => p.Name ?? p.Code).ToList()));
    }

    public async Task<Result<string?>> GetBillingAlertAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        var company = await db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == tenantId.Value, ct);

        if (company is null)
            return Result<string?>.Success(null);

        if (string.Equals(company.Trial, "Sim", StringComparison.OrdinalIgnoreCase))
            return Result<string?>.Success(null);

        if (company.NextBillingDate is null)
            return Result<string?>.Success(null);

        var dueDate = company.NextBillingDate.Value.Date;
        var today = DateTime.UtcNow.Date;

        if (dueDate > today)
            return Result<string?>.Success(null);

        var fee = company.MonthlyFee.ToString("C");
        return dueDate < today
            ? Result<string?>.Success($"Mensalidade vencida desde {dueDate:dd/MM/yyyy}. Valor: {fee}.")
            : Result<string?>.Success($"Mensalidade vence hoje. Valor: {fee}.");
    }

    public async Task<Result<IReadOnlyList<TenantId>>> ListActiveTenantIdsAsync(CancellationToken ct = default)
    {
        var ids = await db.Companies.AsNoTracking()
            .Where(c => c.Active == "Sim")
            .Select(c => c.Id)
            .ToListAsync(ct);

        var tenants = new List<TenantId>();
        foreach (var id in ids)
        {
            var tenantResult = TenantId.Create(id);
            if (tenantResult.IsSuccess)
                tenants.Add(tenantResult.Value);
        }

        return Result<IReadOnlyList<TenantId>>.Success(tenants);
    }
}

public sealed class LegacyDailyDigestLogRepository(LegacySasDbContext db) : IDailyDigestLogRepository
{
    public async Task<Result<bool>> ExistsForDateAsync(
        TenantId tenantId,
        DateOnly digestDate,
        CancellationToken ct = default)
    {
        var exists = await db.DailyDigestLogs.AsNoTracking()
            .AnyAsync(
                row => row.CompanyId == tenantId.Value && row.DigestDate == digestDate,
                ct);

        return Result<bool>.Success(exists);
    }

    public async Task<Result> SaveAsync(DailyDigestLog log, CancellationToken ct = default)
    {
        db.DailyDigestLogs.Add(new LegacyDailyDigestLogRow
        {
            CompanyId = log.TenantId.Value,
            DigestDate = log.DigestDate,
            SentAt = log.SentAt,
        });

        try
        {
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure($"Failed to persist digest log: {ex.InnerException?.Message ?? ex.Message}");
        }
    }
}
