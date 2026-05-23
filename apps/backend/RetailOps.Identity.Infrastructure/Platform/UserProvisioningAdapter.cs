using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Platform.Core.Application.Ports;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Infrastructure.Platform;

public sealed class UserProvisioningAdapter(
    IUserRepository users,
    IPasswordHasher passwordHasher) : IUserProvisioningPort
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        users.ExistsByEmailAsync(email, ct);

    public async Task<Result<ProvisionedAdminUser>> ProvisionTrialAdminAsync(
        int tenantId,
        string name,
        string email,
        string password,
        CancellationToken ct = default)
    {
        var hash = passwordHasher.Hash(password);
        var created = await users.CreateTenantAdminAsync(tenantId, name, email, hash, ct);
        if (created.IsFailure)
            return Result<ProvisionedAdminUser>.Failure(created.Error);

        return Result<ProvisionedAdminUser>.Success(
            new ProvisionedAdminUser(created.Value.UserId, created.Value.LegacyUserId));
    }
}

public sealed class UserDeactivationAdapter(IUserRepository users) : IUserDeactivationPort
{
    public Task<Result> DeactivateUsersByTenantAsync(int tenantId, CancellationToken ct = default) =>
        users.DeactivateByTenantAsync(tenantId, ct);
}
