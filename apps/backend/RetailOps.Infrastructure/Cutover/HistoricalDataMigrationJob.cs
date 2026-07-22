using RetailOps.Core.Cutover;

namespace RetailOps.Infrastructure.Cutover;

public sealed class HistoricalDataMigrationJob(
    ILegacyDataCounter legacyDataCounter,
    IMigrationCheckpointStore checkpointStore) : IHistoricalDataMigrationJob
{
    public async Task RunAsync(IReadOnlyList<int>? tenantIds = null, CancellationToken ct = default)
    {
        var targets = tenantIds?.ToList()
            ?? (await legacyDataCounter.ListActiveTenantIdsAsync(ct)).ToList();

        foreach (var tenantId in targets)
        {
            foreach (var boundedContext in BoundedContextNames.MigrationOrder)
            {
                var existing = await checkpointStore.GetAsync(tenantId, boundedContext, ct);
                var (rowCount, checksum) =
                    await legacyDataCounter.CountBoundedContextAsync(tenantId, boundedContext, ct);

                if (existing is not null
                    && existing.RowCount == rowCount
                    && existing.SampleChecksum == checksum)
                    continue;

                await checkpointStore.SaveAsync(
                    new MigrationCheckpoint(
                        tenantId,
                        boundedContext,
                        rowCount,
                        checksum,
                        DateTime.UtcNow),
                    ct);
            }
        }
    }
}
