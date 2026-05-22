using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Application.Ports;

public interface IUserRepository
{
    Task<User?> FindByEmailOrCpfAsync(string login, CancellationToken ct = default);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListByTenantAsync(int tenantId, CancellationToken ct = default);
    Task<Result> SaveAsync(User user, CancellationToken ct = default);
}
