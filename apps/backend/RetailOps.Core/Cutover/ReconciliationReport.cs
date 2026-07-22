namespace RetailOps.Core.Cutover;

public sealed record ReconciliationLine(
    string BoundedContext,
    string Aggregate,
    long LegacyCount,
    long NormalizedCount,
    long SampleChecksumLegacy,
    long SampleChecksumNormalized,
    bool IsCritical);

public sealed record ReconciliationReport(
    int TenantId,
    DateTime GeneratedAtUtc,
    IReadOnlyList<ReconciliationLine> Lines)
{
    public bool HasCriticalDiscrepancies =>
        Lines.Any(l => l.IsCritical && l.LegacyCount != l.NormalizedCount);
}
