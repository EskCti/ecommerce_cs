using RetailOps.Core.Cutover;

namespace RetailOps.Infrastructure.Cutover;

public sealed class ReconciliationReportService(
    ILegacyDataCounter legacyDataCounter,
    IMigrationCheckpointStore checkpointStore) : IReconciliationReportService
{
    private static readonly (string Bc, string Aggregate, bool Critical)[] Aggregates =
    [
        (BoundedContextNames.Sales, "cart_items", true),
        (BoundedContextNames.Catalog, "products", true),
        (BoundedContextNames.Finance, "receivables", true)
    ];

    public async Task<ReconciliationReport> BuildAsync(int tenantId, CancellationToken ct = default)
    {
        var lines = new List<ReconciliationLine>();
        var checkpoints = (await checkpointStore.ListForTenantAsync(tenantId, ct))
            .ToDictionary(x => x.BoundedContext, x => x);

        foreach (var (bc, aggregate, critical) in Aggregates)
        {
            var (legacyCount, legacyChecksum) =
                await legacyDataCounter.CountBoundedContextAsync(tenantId, bc, ct);

            checkpoints.TryGetValue(bc, out var checkpoint);
            var normalizedCount = checkpoint?.RowCount ?? 0;
            var normalizedChecksum = checkpoint?.SampleChecksum ?? 0;

            lines.Add(new ReconciliationLine(
                bc,
                aggregate,
                legacyCount,
                normalizedCount,
                legacyChecksum,
                normalizedChecksum,
                critical));
        }

        return new ReconciliationReport(tenantId, DateTime.UtcNow, lines);
    }
}
