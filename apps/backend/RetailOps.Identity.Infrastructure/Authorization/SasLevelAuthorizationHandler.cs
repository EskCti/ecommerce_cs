using Microsoft.AspNetCore.Authorization;
using RetailOps.Identity.Core.Domain.Enums;

namespace RetailOps.Identity.Infrastructure.Authorization;

public sealed class SasLevelRequirement : IAuthorizationRequirement;

public sealed class SasLevelAuthorizationHandler : AuthorizationHandler<SasLevelRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SasLevelRequirement requirement)
    {
        var level = context.User.FindFirst("user_level")?.Value;
        if (Enum.TryParse<UserLevel>(level, out var parsed) && parsed == UserLevel.Sas)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
