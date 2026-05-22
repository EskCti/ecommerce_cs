using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;

namespace RetailOps.Identity.Core.Domain.Entities;

public sealed class PermissionGrant : Entity
{
    public Guid UserId { get; private set; }
    public PermissionKey PermissionKey { get; private set; }

    private PermissionGrant(Guid userId, PermissionKey permissionKey)
    {
        UserId = userId;
        PermissionKey = permissionKey;
    }

    public static PermissionGrant Create(Guid userId, PermissionKey permissionKey) =>
        new(userId, permissionKey);
}
