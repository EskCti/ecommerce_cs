using Microsoft.Extensions.Options;
using RetailOps.Core.Migration;

namespace RetailOps.Infrastructure.Migration;

public sealed class MigrationSettings : IMigrationSettings
{
    public MigrationSettings(IOptions<MigrationOptions> options)
    {
        var value = options.Value;
        PilotTenantIds = value.PilotTenantIds;
        FullyMigratedTenantIds = value.FullyMigratedTenantIds;
        LegacyPhpEnabled = value.LegacyPhpEnabled;
        AllTenantsOnRetailOps = value.AllTenantsOnRetailOps;
        DualWriteEnabled = value.DualWriteEnabled;
        BoundedContextFlags = value.BoundedContexts;
    }

    public IReadOnlyList<int> PilotTenantIds { get; }
    public IReadOnlyList<int> FullyMigratedTenantIds { get; }
    public bool LegacyPhpEnabled { get; }
    public bool AllTenantsOnRetailOps { get; }
    public bool DualWriteEnabled { get; }
    public IReadOnlyDictionary<string, bool> BoundedContextFlags { get; }

    public bool IsTenantFullyMigrated(int tenantId) =>
        AllTenantsOnRetailOps || FullyMigratedTenantIds.Contains(tenantId);

    public int CountTenantsOnPhpPath(IReadOnlyList<int> activeTenantIds)
    {
        if (!LegacyPhpEnabled && AllTenantsOnRetailOps)
            return 0;

        if (LegacyPhpEnabled)
            return activeTenantIds.Count(id => !IsTenantFullyMigrated(id));

        return activeTenantIds.Count(id => !IsTenantFullyMigrated(id));
    }
}
