using Microsoft.EntityFrameworkCore;
using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Infrastructure.Seeds;
using RetailOps.Infrastructure.Legacy.Persistence;

namespace RetailOps.Identity.Infrastructure.Queries;

public sealed class PermissionCatalogQuery(LegacySasDbContext db) : IPermissionCatalogQuery
{
    public async Task<IReadOnlyList<PermissionCatalogItemDto>> ListAsync(CancellationToken ct = default)
    {
        try
        {
            var fromDb = await db.AccessCatalog.AsNoTracking()
                .OrderBy(a => a.GroupId).ThenBy(a => a.Name)
                .Select(a => new PermissionCatalogItemDto(a.Key, a.Name, a.GroupId.ToString()))
                .ToListAsync(ct);

            if (fromDb.Count > 0)
                return fromDb;
        }
        catch
        {
            // fallback when legacy DB unavailable
        }

        return PermissionCatalogSeed.Items;
    }
}
