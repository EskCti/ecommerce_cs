using Microsoft.AspNetCore.Authorization;
using RetailOps.Identity.Core.Domain.Enums;
using DomainAuthPolicy = RetailOps.Identity.Core.Domain.Services.AuthorizationPolicy;

namespace RetailOps.Identity.Infrastructure.Authorization;

public sealed class PermissionRequirement(string permissionKey) : IAuthorizationRequirement
{
    public string PermissionKey { get; } = permissionKey;
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var level = context.User.FindFirst("user_level")?.Value;
        if (Enum.TryParse<UserLevel>(level, out var parsed) && DomainAuthPolicy.IsPrivileged(parsed))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.HasClaim(c => c.Type == "permission_keys" && c.Value == "*"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var hasKey = context.User.Claims.Any(c =>
            c.Type == "permission_keys"
            && c.Value.Equals(requirement.PermissionKey, StringComparison.OrdinalIgnoreCase));

        if (hasKey)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
