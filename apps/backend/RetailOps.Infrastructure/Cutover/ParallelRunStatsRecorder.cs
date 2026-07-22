using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Cutover;
using RetailOps.Infrastructure.Persistence.Contexts;
using RetailOps.Infrastructure.Persistence.Entities;

namespace RetailOps.Infrastructure.Cutover;

public sealed class ParallelRunStatsRecorder(RetailOpsDbContext db) : IParallelRunStatsRecorder
{
    public async Task RecordComparisonAsync(
        int tenantId,
        decimal divergencePercent,
        CancellationToken ct = default)
    {
        db.ParallelRunStats.Add(new ParallelRunStatRow
        {
            TenantId = tenantId,
            DivergencePercent = divergencePercent,
            RecordedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<(int SampleCount, decimal MaxDivergencePercent, int DaysCovered)> GetSummaryAsync(
        int minimumDays,
        CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-minimumDays);
        var rows = await db.ParallelRunStats.AsNoTracking()
            .Where(x => x.RecordedAtUtc >= since)
            .ToListAsync(ct);

        if (rows.Count == 0)
            return (0, 0m, 0);

        var daysCovered = rows
            .Select(x => x.RecordedAtUtc.Date)
            .Distinct()
            .Count();

        return (rows.Count, rows.Max(x => x.DivergencePercent), daysCovered);
    }
}
