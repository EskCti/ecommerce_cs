using RetailOps.Core.Cutover;
using RetailOps.Core.Migration;

namespace RetailOps.Infrastructure.Cutover;

public sealed class CutoverGateService(
    IMigrationSettings migrationSettings,
    IParallelRunStatsRecorder parallelRunStats) : ICutoverGateService
{
    private const decimal SalesParallelRunThresholdPercent = 0.1m;
    private const int MinimumParallelRunDays = 14;

    public async Task<CutoverChecklistResult> RunPreCutoverChecklistAsync(CancellationToken ct = default)
    {
        var items = new List<CutoverCheckItem>();

        foreach (var module in BoundedContextNames.CutoverModuleFlags)
        {
            var enabled = migrationSettings.BoundedContextFlags.TryGetValue(module, out var flag) && flag;
            items.Add(new CutoverCheckItem(
                $"module:{module}",
                enabled,
                enabled ? "enabled" : "Migration module flag is false"));
        }

        var (sampleCount, maxDivergence, daysCovered) =
            await parallelRunStats.GetSummaryAsync(MinimumParallelRunDays, ct);

        var parallelRunOk = sampleCount > 0
            && daysCovered >= MinimumParallelRunDays
            && maxDivergence <= SalesParallelRunThresholdPercent;

        items.Add(new CutoverCheckItem(
            "sales:parallel-run",
            parallelRunOk,
            sampleCount == 0
                ? $"No parallel-run samples in last {MinimumParallelRunDays} days"
                : $"max divergence {maxDivergence:F4}% over {daysCovered} day(s), {sampleCount} sample(s)"));

        var phpDisabled = !migrationSettings.LegacyPhpEnabled;
        items.Add(new CutoverCheckItem(
            "traffic:legacy-php-disabled",
            phpDisabled,
            phpDisabled ? "LegacyPhpEnabled is false" : "LegacyPhpEnabled must be false before cutover"));

        var allTenantsMigrated = migrationSettings.AllTenantsOnRetailOps;
        items.Add(new CutoverCheckItem(
            "tenants:all-on-retailops",
            allTenantsMigrated,
            allTenantsMigrated
                ? "AllTenantsOnRetailOps is true"
                : "Set AllTenantsOnRetailOps or complete tenant waves"));

        return new CutoverChecklistResult(
            items.All(i => i.Passed),
            items);
    }
}
