using RetailOps.Core.Returns.Application.DTOs;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Returns.Application.Queries;

public interface IListExchangesQuery
{
    Task<Result<ExchangeListPageDto>> ListByTenantAsync(
        TenantId tenantId,
        ExchangeListFilter filter,
        CancellationToken ct = default);
}
