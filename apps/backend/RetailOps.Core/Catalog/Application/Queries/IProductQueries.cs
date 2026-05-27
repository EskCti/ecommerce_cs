using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.Queries;

public interface IProductQueries
{
    Task<Result<PaginatedProductsOutputDto>> ListByTenantAsync(
        TenantId tenantId,
        ProductListFilter? filter = null,
        CancellationToken ct = default);
}

public sealed record ProductListFilter(
    string? Barcode,
    string? Name,
    Guid? CategoryId,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20);

public interface ICategoryQueries
{
    Task<Result<IReadOnlyList<CategoryListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        CategoryListFilter? filter = null,
        CancellationToken ct = default);
}

public sealed record CategoryListFilter(
    string? Name,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20);

public interface IListLowStockProductsQuery
{
    Task<Result<IReadOnlyList<LowStockProductDto>>> ExecuteAsync(
        TenantId tenantId,
        CancellationToken ct = default);
}
