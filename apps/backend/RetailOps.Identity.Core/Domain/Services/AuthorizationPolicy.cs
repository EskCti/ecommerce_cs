using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;

namespace RetailOps.Identity.Core.Domain.Services;

public static class AuthorizationPolicy
{
    public static bool IsPrivileged(UserLevel level) =>
        level is UserLevel.Sas or UserLevel.Administrador;

    public static bool RequiresGrants(UserLevel level) => !IsPrivileged(level);

    public static bool CanLogin(User user)
    {
        if (user.Status == ActiveStatus.Inactive)
            return false;

        if (IsPrivileged(user.Level))
            return true;

        return user.Grants.Count > 0;
    }

    public static bool HasPermission(User user, string permissionKey)
    {
        if (IsPrivileged(user.Level))
            return true;

        return user.Grants.Any(g => g.PermissionKey.Value == permissionKey);
    }
}
