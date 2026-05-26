using RetailOps.Shared.Kernel.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Entities;

namespace RetailOps.Core.StoreSettings.Domain.Repositories;

public interface IStoreConfigRepository : IRepository<StoreConfig>
{
    Task<Result<StoreConfig>> GetByTenantId(TenantId tenantId);
    Task<Result<bool>> ExistsForTenant(TenantId tenantId);
}