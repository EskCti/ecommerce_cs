using RetailOps.Core.Returns.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Returns.Domain.Repositories;

public interface IExchangeRepository
{
    Task<Result<Exchange>> GetById(TenantId tenantId, Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Exchange>>> ListByTenant(TenantId tenantId, CancellationToken ct = default);
    Task<Result> Save(Exchange exchange, CancellationToken ct = default);
    Task<Result> Delete(TenantId tenantId, Guid id, CancellationToken ct = default);
}
