namespace RetailOps.Core.Cutover;

public sealed record MigrationCheckpoint(
    int TenantId,
    string BoundedContext,
    long RowCount,
    long SampleChecksum,
    DateTime CompletedAtUtc);

public interface IMigrationCheckpointStore
{
    Task<MigrationCheckpoint?> GetAsync(int tenantId, string boundedContext, CancellationToken ct = default);
    Task SaveAsync(MigrationCheckpoint checkpoint, CancellationToken ct = default);
    Task<IReadOnlyList<MigrationCheckpoint>> ListForTenantAsync(int tenantId, CancellationToken ct = default);
}
