namespace RetailOps.Core.Migration;

public interface IMigrationSettings
{
    IReadOnlyList<int> PilotTenantIds { get; }
    IReadOnlyList<int> FullyMigratedTenantIds { get; }
    bool LegacyPhpEnabled { get; }
    bool AllTenantsOnRetailOps { get; }
    bool DualWriteEnabled { get; }
    IReadOnlyDictionary<string, bool> BoundedContextFlags { get; }

    bool IsTenantFullyMigrated(int tenantId);
    int CountTenantsOnPhpPath(IReadOnlyList<int> activeTenantIds);
}
