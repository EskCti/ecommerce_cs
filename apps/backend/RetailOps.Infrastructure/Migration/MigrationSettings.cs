using Microsoft.Extensions.Options;
using RetailOps.Core.Migration;

namespace RetailOps.Infrastructure.Migration;

public sealed class MigrationSettings : IMigrationSettings
{
    public MigrationSettings(IOptions<MigrationOptions> options)
    {
        var value = options.Value;
        PilotTenantIds = value.PilotTenantIds;
        BoundedContextFlags = value.BoundedContexts;
    }

    public IReadOnlyList<int> PilotTenantIds { get; }
    public IReadOnlyDictionary<string, bool> BoundedContextFlags { get; }
}
