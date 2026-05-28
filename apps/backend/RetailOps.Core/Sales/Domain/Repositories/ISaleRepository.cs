using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Domain.Repositories;

public interface ISaleRepository : IRepository<Sale>
{
    Task<Result<IReadOnlyList<Sale>>> List(TenantId tenantId, CancellationToken ct = default);
}
