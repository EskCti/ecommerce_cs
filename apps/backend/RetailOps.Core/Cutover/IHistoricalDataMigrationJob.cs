namespace RetailOps.Core.Cutover;

public interface ILegacyDataCounter
{
    Task<IReadOnlyList<int>> ListActiveTenantIdsAsync(CancellationToken ct = default);

    Task<(long RowCount, long SampleChecksum)> CountBoundedContextAsync(
        int tenantId,
        string boundedContext,
        CancellationToken ct = default);
}

public interface IHistoricalDataMigrationJob
{
    Task RunAsync(IReadOnlyList<int>? tenantIds = null, CancellationToken ct = default);
}

public interface IReconciliationReportService
{
    Task<ReconciliationReport> BuildAsync(int tenantId, CancellationToken ct = default);
}
