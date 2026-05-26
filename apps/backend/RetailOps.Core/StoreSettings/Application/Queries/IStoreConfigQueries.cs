using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.Queries;

public interface IStoreConfigQueries
{
    Task<Result<StoreConfigOutputDto?>> GetByTenantAsync(
        TenantId tenantId,
        CancellationToken ct = default);
}