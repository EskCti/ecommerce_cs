using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Catalog;

public sealed class ProductQueries(IProductRepository repository) : IProductQueries
{
    public async Task<Result<PaginatedProductsOutputDto>> ListByTenantAsync(
        TenantId tenantId,
        ProductListFilter? filter = null,
        CancellationToken ct = default)
    {
        var result = await repository.GetByTenantId(tenantId);
        if (result.IsFailure)
            return Result<PaginatedProductsOutputDto>.Failure(result.Error);

        var items = result.Value.AsEnumerable();

        if (filter?.IsActive is bool isActive)
            items = items.Where(p => p.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(filter?.Barcode))
        {
            var term = filter.Barcode.Trim();
            items = items.Where(p => p.Barcode.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter?.Name))
        {
            var term = filter.Name.Trim();
            items = items.Where(p => p.Name.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (filter?.CategoryId is Guid categoryId)
            items = items.Where(p => p.CategoryId.Value == categoryId);

        var page = Math.Max(1, filter?.Page ?? 1);
        var pageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 100);
        var ordered = items.OrderBy(p => p.Name.Value).ToList();
        var totalCount = ordered.Count;

        var projected = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProductListItemDto.FromDomain)
            .ToList();

        return Result<PaginatedProductsOutputDto>.Success(new PaginatedProductsOutputDto
        {
            Items = projected,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }
}
