namespace RetailOps.Core.Migration;

public interface IMigrationSettings
{
    IReadOnlyList<int> PilotTenantIds { get; }
    IReadOnlyDictionary<string, bool> BoundedContextFlags { get; }
}
