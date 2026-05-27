using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Catalog;

public sealed class CategoryQueries(ICategoryRepository repository) : ICategoryQueries
{
    public async Task<Result<IReadOnlyList<CategoryListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        CategoryListFilter? filter = null,
        CancellationToken ct = default)
    {
        var result = await repository.GetByTenantId(tenantId);
        if (result.IsFailure)
            return Result<IReadOnlyList<CategoryListItemDto>>.Failure(result.Error);

        var items = result.Value.AsEnumerable();

        if (filter?.IsActive is bool isActive)
            items = items.Where(c => c.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(filter?.Name))
        {
            var term = filter.Name.Trim();
            items = items.Where(c => c.Name.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var page = Math.Max(1, filter?.Page ?? 1);
        var pageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 100);

        var projected = items
            .OrderBy(c => c.Name.Value)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(CategoryListItemDto.FromDomain)
            .ToList();

        return Result<IReadOnlyList<CategoryListItemDto>>.Success(projected);
    }
}

public sealed class LowStockQueries(IProductRepository repository) : IListLowStockProductsQuery
{
    public async Task<Result<IReadOnlyList<LowStockProductDto>>> ExecuteAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        var result = await repository.GetByTenantId(tenantId);
        if (result.IsFailure)
            return Result<IReadOnlyList<LowStockProductDto>>.Failure(result.Error);

        var projected = result.Value
            .Where(p => p.IsActive && p.IsLowStock())
            .OrderBy(p => p.Stock.Value)
            .Select(LowStockProductDto.FromDomain)
            .ToList();

        return Result<IReadOnlyList<LowStockProductDto>>.Success(projected);
    }
}
