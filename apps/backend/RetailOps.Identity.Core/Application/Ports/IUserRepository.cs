using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Application.Ports;

public interface IUserRepository
{
    Task<User?> FindByEmailOrCpfAsync(string login, CancellationToken ct = default);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListByTenantAsync(int tenantId, CancellationToken ct = default);
    Task<Result> SaveAsync(User user, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<Result<(Guid UserId, int LegacyUserId)>> CreateTenantAdminAsync(
        int tenantId,
        string name,
        string email,
        string passwordHash,
        CancellationToken ct = default);
    Task<Result> DeactivateByTenantAsync(int tenantId, CancellationToken ct = default);
}
