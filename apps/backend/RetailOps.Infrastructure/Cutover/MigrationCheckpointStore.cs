using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Cutover;
using RetailOps.Infrastructure.Persistence.Contexts;
using RetailOps.Infrastructure.Persistence.Entities;

namespace RetailOps.Infrastructure.Cutover;

public sealed class MigrationCheckpointStore(RetailOpsDbContext db) : IMigrationCheckpointStore
{
    public async Task<MigrationCheckpoint?> GetAsync(
        int tenantId,
        string boundedContext,
        CancellationToken ct = default)
    {
        var row = await db.MigrationCheckpoints.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.BoundedContext == boundedContext,
                ct);

        return row is null
            ? null
            : new MigrationCheckpoint(
                row.TenantId,
                row.BoundedContext,
                row.RowCount,
                row.SampleChecksum,
                row.CompletedAtUtc);
    }

    public async Task SaveAsync(MigrationCheckpoint checkpoint, CancellationToken ct = default)
    {
        var existing = await db.MigrationCheckpoints
            .FirstOrDefaultAsync(
                x => x.TenantId == checkpoint.TenantId && x.BoundedContext == checkpoint.BoundedContext,
                ct);

        if (existing is null)
        {
            db.MigrationCheckpoints.Add(new MigrationCheckpointRow
            {
                TenantId = checkpoint.TenantId,
                BoundedContext = checkpoint.BoundedContext,
                RowCount = checkpoint.RowCount,
                SampleChecksum = checkpoint.SampleChecksum,
                CompletedAtUtc = checkpoint.CompletedAtUtc
            });
        }
        else
        {
            existing.RowCount = checkpoint.RowCount;
            existing.SampleChecksum = checkpoint.SampleChecksum;
            existing.CompletedAtUtc = checkpoint.CompletedAtUtc;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MigrationCheckpoint>> ListForTenantAsync(
        int tenantId,
        CancellationToken ct = default)
    {
        var rows = await db.MigrationCheckpoints.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.BoundedContext)
            .ToListAsync(ct);

        return rows.Select(row => new MigrationCheckpoint(
            row.TenantId,
            row.BoundedContext,
            row.RowCount,
            row.SampleChecksum,
            row.CompletedAtUtc)).ToList();
    }
}
