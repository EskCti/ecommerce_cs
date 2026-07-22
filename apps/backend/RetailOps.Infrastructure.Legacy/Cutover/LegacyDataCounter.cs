using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Cutover;
using RetailOps.Infrastructure.Legacy.Persistence;

namespace RetailOps.Infrastructure.Legacy.Cutover;

public sealed class LegacyDataCounter(LegacySasDbContext db) : ILegacyDataCounter
{
    public async Task<IReadOnlyList<int>> ListActiveTenantIdsAsync(CancellationToken ct = default)
    {
        return await db.Companies.AsNoTracking()
            .Where(c => c.Active == "sim")
            .Select(c => c.Id)
            .OrderBy(id => id)
            .ToListAsync(ct);
    }

    public async Task<(long RowCount, long SampleChecksum)> CountBoundedContextAsync(
        int tenantId,
        string boundedContext,
        CancellationToken ct = default)
    {
        return boundedContext switch
        {
            BoundedContextNames.Platform => await CountIds(
                db.Companies.AsNoTracking().Where(x => x.Id == tenantId).Select(x => x.Id),
                ct),
            BoundedContextNames.Identity => await CountIds(
                db.Users.AsNoTracking().Where(x => x.CompanyId == tenantId).Select(x => x.Id),
                ct),
            BoundedContextNames.Settings => await CountIds(
                db.Configs.AsNoTracking().Where(x => x.CompanyId == tenantId).Select(x => x.Id),
                ct),
            BoundedContextNames.Catalog => await CountIds(
                db.Products.AsNoTracking().Where(x => x.CompanyId == tenantId).Select(x => x.Id),
                ct),
            BoundedContextNames.Crm => await CountCrm(tenantId, ct),
            BoundedContextNames.Sales => await CountIds(
                db.CartItems.AsNoTracking().Where(x => x.CompanyId == tenantId).Select(x => x.Id),
                ct),
            BoundedContextNames.Finance => await CountIds(
                db.Receivables.AsNoTracking().Where(x => x.CompanyId == tenantId).Select(x => x.Id),
                ct),
            BoundedContextNames.Returns => await CountIds(
                db.Exchanges.AsNoTracking().Where(x => x.CompanyId == tenantId).Select(x => x.Id),
                ct),
            _ => (0, 0)
        };
    }

    private async Task<(long, long)> CountCrm(int tenantId, CancellationToken ct)
    {
        var customerIds = await db.Customers.AsNoTracking()
            .Where(x => x.CompanyId == tenantId)
            .Select(x => x.Id)
            .ToListAsync(ct);
        var supplierIds = await db.Suppliers.AsNoTracking()
            .Where(x => x.CompanyId == tenantId)
            .Select(x => x.Id)
            .ToListAsync(ct);
        var all = customerIds.Concat(supplierIds).ToList();
        return (all.Count, all.Sum(i => (long)i));
    }

    private static async Task<(long RowCount, long SampleChecksum)> CountIds(
        IQueryable<int> idQuery,
        CancellationToken ct)
    {
        var ids = await idQuery.ToListAsync(ct);
        return (ids.Count, ids.Sum(i => (long)i));
    }
}
