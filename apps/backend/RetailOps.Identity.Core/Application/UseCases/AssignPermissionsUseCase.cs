using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Application.UseCases;

public sealed class AssignPermissionsUseCase(IUserRepository users) : IUseCase<AssignPermissionsInDto>
{
    public async Task<Result> Execute(AssignPermissionsInDto input)
    {
        var user = await users.FindByIdAsync(input.UserId);
        if (user is null)
            return Result.Failure("User not found.");

        var desired = new HashSet<string>(input.PermissionKeys.Select(k => k.Trim().ToLowerInvariant()));
        var current = user.Grants.Select(g => g.PermissionKey.Value).ToHashSet();

        foreach (var key in current.Where(k => !desired.Contains(k)))
        {
            var pk = PermissionKey.Create(key);
            if (pk.IsFailure) continue;
            var revoke = user.RevokePermission(pk.Value);
            if (revoke.IsFailure) return revoke;
        }

        foreach (var key in desired.Where(k => !current.Contains(k)))
        {
            var pk = PermissionKey.Create(key);
            if (pk.IsFailure) return Result.Failure(pk.Error);
            var assign = user.AssignPermission(pk.Value);
            if (assign.IsFailure) return assign;
        }

        return await users.SaveAsync(user);
    }
}
