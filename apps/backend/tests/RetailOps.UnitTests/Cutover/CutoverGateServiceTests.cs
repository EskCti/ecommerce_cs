using RetailOps.Core.Cutover;
using RetailOps.Core.Migration;
using RetailOps.Infrastructure.Cutover;
using Xunit;

namespace RetailOps.UnitTests.Cutover;

public class CutoverGateServiceTests
{
    [Fact]
    public async Task RunPreCutoverChecklist_Fails_WhenModulesDisabled()
    {
        var settings = new StubMigrationSettings(
            legacyPhpEnabled: false,
            allTenantsOnRetailOps: true,
            boundedContexts: new Dictionary<string, bool> { ["auth"] = false });

        var sut = new CutoverGateService(settings, new StubParallelRunStats(15, 0.05m, 14));
        var result = await sut.RunPreCutoverChecklistAsync();

        Assert.False(result.CanProceed);
        Assert.Contains(result.Items, i => i.Name == "module:auth" && !i.Passed);
    }

    [Fact]
    public async Task RunPreCutoverChecklist_Passes_WhenAllGatesGreen()
    {
        var flags = BoundedContextNames.CutoverModuleFlags
            .ToDictionary(x => x, _ => true);

        var settings = new StubMigrationSettings(
            legacyPhpEnabled: false,
            allTenantsOnRetailOps: true,
            boundedContexts: flags);

        var sut = new CutoverGateService(settings, new StubParallelRunStats(100, 0.05m, 14));
        var result = await sut.RunPreCutoverChecklistAsync();

        Assert.True(result.CanProceed);
    }

    private sealed class StubMigrationSettings : IMigrationSettings
    {
        public StubMigrationSettings(
            bool legacyPhpEnabled,
            bool allTenantsOnRetailOps,
            Dictionary<string, bool> boundedContexts)
        {
            LegacyPhpEnabled = legacyPhpEnabled;
            AllTenantsOnRetailOps = allTenantsOnRetailOps;
            BoundedContextFlags = boundedContexts;
        }

        public IReadOnlyList<int> PilotTenantIds => [];
        public IReadOnlyList<int> FullyMigratedTenantIds => [];
        public bool LegacyPhpEnabled { get; }
        public bool AllTenantsOnRetailOps { get; }
        public bool DualWriteEnabled => false;
        public IReadOnlyDictionary<string, bool> BoundedContextFlags { get; }

        public bool IsTenantFullyMigrated(int tenantId) => AllTenantsOnRetailOps;
        public int CountTenantsOnPhpPath(IReadOnlyList<int> activeTenantIds) => 0;
    }

    private sealed class StubParallelRunStats(int count, decimal max, int days) : IParallelRunStatsRecorder
    {
        public Task RecordComparisonAsync(int tenantId, decimal divergencePercent, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<(int SampleCount, decimal MaxDivergencePercent, int DaysCovered)> GetSummaryAsync(
            int minimumDays,
            CancellationToken ct = default)
            => Task.FromResult((count, max, days));
    }
}
