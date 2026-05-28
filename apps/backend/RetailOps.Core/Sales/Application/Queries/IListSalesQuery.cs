using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.Queries;

public interface IListSalesQuery
{
    Task<Result<SaleListPageDto>> ListByTenantAsync(
        TenantId tenantId,
        SaleListFilter filter,
        CancellationToken ct = default);
}
