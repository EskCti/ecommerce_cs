namespace RetailOps.Infrastructure.Migration;

public sealed class MigrationOptions
{
    public int[] PilotTenantIds { get; init; } = [];
    public int[] FullyMigratedTenantIds { get; init; } = [];
    public bool LegacyPhpEnabled { get; init; } = true;
    public bool AllTenantsOnRetailOps { get; init; }
    public bool DualWriteEnabled { get; init; } = true;
    public Dictionary<string, bool> BoundedContexts { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
