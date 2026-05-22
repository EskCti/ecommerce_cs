using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;

namespace RetailOps.Identity.Core.Domain.Entities;

public sealed class PermissionDefinition : Entity
{
    public PermissionKey Key { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string GroupName { get; private set; } = string.Empty;

    public PermissionDefinition(PermissionKey key, string name, string groupName)
    {
        Key = key;
        Name = name;
        GroupName = groupName;
    }
}
