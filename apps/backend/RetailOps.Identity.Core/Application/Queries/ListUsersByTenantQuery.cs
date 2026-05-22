using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Application.Queries;

public sealed class ListUsersByTenantQuery(IUserRepository users)
{
    public async Task<Result<IReadOnlyList<UserListItemDto>>> Execute(int tenantId, CancellationToken ct = default)
    {
        var list = await users.ListByTenantAsync(tenantId, ct);
        var dtos = list.Select(u => new UserListItemDto(
            u.Id,
            u.LegacyUserId,
            u.Name,
            u.Email?.Value,
            u.Level.ToString(),
            u.Grants.Select(g => g.PermissionKey.Value).ToList())).ToList();

        return Result<IReadOnlyList<UserListItemDto>>.Success(dtos);
    }
}
