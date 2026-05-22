namespace RetailOps.Infrastructure.Migration;

public sealed class MigrationOptions
{
    public int[] PilotTenantIds { get; init; } = [];
    public Dictionary<string, bool> BoundedContexts { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
