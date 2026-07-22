namespace RetailOps.Core.Cutover;

public interface IParallelRunStatsRecorder
{
    Task RecordComparisonAsync(
        int tenantId,
        decimal divergencePercent,
        CancellationToken ct = default);

    Task<(int SampleCount, decimal MaxDivergencePercent, int DaysCovered)> GetSummaryAsync(
        int minimumDays,
        CancellationToken ct = default);
}
