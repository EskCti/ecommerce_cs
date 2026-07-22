namespace RetailOps.Infrastructure.Persistence.Entities;

public sealed class MigrationCheckpointRow
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string BoundedContext { get; set; } = string.Empty;
    public long RowCount { get; set; }
    public long SampleChecksum { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}

public sealed class ParallelRunStatRow
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public decimal DivergencePercent { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}
