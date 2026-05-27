using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.IntegrationTests.Support;

internal sealed class CatalogTestState
{
    public Dictionary<Guid, Product> Products { get; } = new();
    public Dictionary<Guid, Category> Categories { get; } = new();
    public Dictionary<Guid, StockMovement> StockMovements { get; } = new();
    public int ProductLegacyCounter { get; set; }
    public int CategoryLegacyCounter { get; set; }

    public void Reset()
    {
        Products.Clear();
        Categories.Clear();
        StockMovements.Clear();
        ProductLegacyCounter = 0;
        CategoryLegacyCounter = 0;
    }
}

internal sealed class InMemoryProductRepository(CatalogTestState state) : IProductRepository
{
    public Task<Result<Product>> GetById(Guid id) =>
        Task.FromResult(state.Products.TryGetValue(id, out var product)
            ? Result<Product>.Success(product)
            : Result<Product>.Failure("Product not found."));

    public Task<Result<Product?>> FindByBarcode(TenantId tenantId, Barcode barcode)
    {
        var match = state.Products.Values.FirstOrDefault(p =>
            p.TenantId.Value == tenantId.Value
            && string.Equals(p.Barcode.Value, barcode.Value, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(Result<Product?>.Success(match));
    }

    public Task<Result<bool>> ExistsBarcode(
        TenantId tenantId,
        Barcode barcode,
        Guid? excludeProductId = null)
    {
        var exists = state.Products.Values.Any(p =>
            p.TenantId.Value == tenantId.Value
            && string.Equals(p.Barcode.Value, barcode.Value, StringComparison.OrdinalIgnoreCase)
            && (excludeProductId is null || p.Id != excludeProductId));

        return Task.FromResult(Result<bool>.Success(exists));
    }

    public Task<Result<IEnumerable<Product>>> GetByTenantId(TenantId tenantId)
    {
        var items = state.Products.Values.Where(p => p.TenantId.Value == tenantId.Value);
        return Task.FromResult(Result<IEnumerable<Product>>.Success(items));
    }

    public Task<Result> Save(Product entity)
    {
        if (!state.Products.ContainsKey(entity.Id))
        {
            state.ProductLegacyCounter++;
            entity.SyncIdentity(Guid.Parse($"00000000-0000-0000-0009-{state.ProductLegacyCounter:D12}"));
        }

        state.Products[entity.Id] = entity;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Product cannot be deleted. Use deactivation instead."));
}

internal sealed class InMemoryCategoryRepository(CatalogTestState state) : ICategoryRepository
{
    public Task<Result<Category>> GetById(Guid id) =>
        Task.FromResult(state.Categories.TryGetValue(id, out var category)
            ? Result<Category>.Success(category)
            : Result<Category>.Failure("Category not found."));

    public Task<Result<IEnumerable<Category>>> GetByTenantId(TenantId tenantId)
    {
        var items = state.Categories.Values.Where(c => c.TenantId.Value == tenantId.Value);
        return Task.FromResult(Result<IEnumerable<Category>>.Success(items));
    }

    public Task<Result> Save(Category entity)
    {
        if (!state.Categories.ContainsKey(entity.Id))
        {
            state.CategoryLegacyCounter++;
            entity.SyncIdentity(Guid.Parse($"00000000-0000-0000-0010-{state.CategoryLegacyCounter:D12}"));
        }

        state.Categories[entity.Id] = entity;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Category cannot be deleted. Use deactivation instead."));
}

internal sealed class InMemoryStockMovementRepository(CatalogTestState state) : IStockMovementRepository
{
    public Task<Result> Save(StockMovement movement)
    {
        state.StockMovements[movement.Id] = movement;
        return Task.FromResult(Result.Success());
    }

    public Task<Result<IReadOnlyList<StockMovement>>> GetByProductId(Guid productId)
    {
        var items = state.StockMovements.Values
            .Where(m => m.ProductId == productId)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<StockMovement>>.Success(items));
    }
}

internal sealed class InMemoryCatalogLegacyPort(CatalogTestState state) : ICatalogLegacyPort
{
    public Task<Result<IReadOnlyList<Product>>> GetProductsFromLegacyAsync(TenantId tenantId, CancellationToken ct = default)
    {
        var items = state.Products.Values.Where(p => p.TenantId.Value == tenantId.Value).ToList();
        return Task.FromResult(Result<IReadOnlyList<Product>>.Success(items));
    }

    public Task<Result<Product?>> GetProductFromLegacyAsync(TenantId tenantId, Guid productId, CancellationToken ct = default)
    {
        if (!state.Products.TryGetValue(productId, out var product) || product.TenantId.Value != tenantId.Value)
            return Task.FromResult(Result<Product?>.Success(null));

        return Task.FromResult(Result<Product?>.Success(product));
    }

    public Task<Result<Product?>> GetProductByBarcodeFromLegacyAsync(TenantId tenantId, string barcode, CancellationToken ct = default)
    {
        var match = state.Products.Values.FirstOrDefault(p =>
            p.TenantId.Value == tenantId.Value
            && string.Equals(p.Barcode.Value, barcode.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(Result<Product?>.Success(match));
    }

    public Task<Result<int>> SaveProductToLegacyAsync(Product product, CancellationToken ct = default) =>
        Task.FromResult(Result<int>.Success(1));

    public Task<Result<IReadOnlyList<Category>>> GetCategoriesFromLegacyAsync(TenantId tenantId, CancellationToken ct = default)
    {
        var items = state.Categories.Values.Where(c => c.TenantId.Value == tenantId.Value).ToList();
        return Task.FromResult(Result<IReadOnlyList<Category>>.Success(items));
    }

    public Task<Result<Category?>> GetCategoryFromLegacyAsync(TenantId tenantId, Guid categoryId, CancellationToken ct = default)
    {
        if (!state.Categories.TryGetValue(categoryId, out var category) || category.TenantId.Value != tenantId.Value)
            return Task.FromResult(Result<Category?>.Success(null));

        return Task.FromResult(Result<Category?>.Success(category));
    }

    public Task<Result<int>> SaveCategoryToLegacyAsync(Category category, CancellationToken ct = default) =>
        Task.FromResult(Result<int>.Success(1));

    public Task<Result<int>> SaveStockMovementToLegacyAsync(StockMovement movement, TenantId tenantId, CancellationToken ct = default) =>
        Task.FromResult(Result<int>.Success(1));

    public Task<Result<IReadOnlyList<StockMovement>>> GetStockMovementsFromLegacyAsync(Guid productId, CancellationToken ct = default)
    {
        var items = state.StockMovements.Values.Where(m => m.ProductId == productId).ToList();
        return Task.FromResult(Result<IReadOnlyList<StockMovement>>.Success(items));
    }
}

internal sealed class InMemoryStockLegacyPort(CatalogTestState state) : IStockLegacyPort
{
    public Task<Result> AdjustStockAsync(TenantId tenantId, Guid productId, int newQuantity, CancellationToken ct = default)
    {
        if (!state.Products.TryGetValue(productId, out var product) || product.TenantId.Value != tenantId.Value)
            return Task.FromResult(Result.Failure("Product not found."));

        var stockResult = StockQuantity.Create(newQuantity);
        if (stockResult.IsFailure)
            return Task.FromResult(Result.Failure(stockResult.Error));

        var adjustResult = product.AdjustStock(stockResult.Value, new StockAdjustmentPolicy());
        return Task.FromResult(adjustResult);
    }

    public Task<Result> ReserveStockAsync(TenantId tenantId, Guid productId, int quantity, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> ReleaseStockAsync(TenantId tenantId, Guid productId, int quantity, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

internal sealed class InMemoryProductQueries(CatalogTestState state) : IProductQueries
{
    public Task<Result<PaginatedProductsOutputDto>> ListByTenantAsync(
        TenantId tenantId,
        ProductListFilter? filter = null,
        CancellationToken ct = default)
    {
        var items = state.Products.Values.Where(p => p.TenantId.Value == tenantId.Value).AsEnumerable();

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

        return Task.FromResult(Result<PaginatedProductsOutputDto>.Success(new PaginatedProductsOutputDto
        {
            Items = projected,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        }));
    }
}

internal sealed class InMemoryCategoryQueries(CatalogTestState state) : ICategoryQueries
{
    public Task<Result<IReadOnlyList<CategoryListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        CategoryListFilter? filter = null,
        CancellationToken ct = default)
    {
        var items = state.Categories.Values.Where(c => c.TenantId.Value == tenantId.Value).AsEnumerable();

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

        return Task.FromResult(Result<IReadOnlyList<CategoryListItemDto>>.Success(projected));
    }
}

internal sealed class InMemoryCatalogFixture
{
    public CatalogTestState State { get; } = new();
    public InMemoryProductRepository ProductRepository { get; }
    public InMemoryCategoryRepository CategoryRepository { get; }
    public InMemoryStockMovementRepository StockMovementRepository { get; }
    public InMemoryCatalogLegacyPort LegacyPort { get; }
    public InMemoryStockLegacyPort StockLegacyPort { get; }
    public InMemoryProductQueries ProductQueries { get; }
    public InMemoryCategoryQueries CategoryQueries { get; }

    public InMemoryCatalogFixture()
    {
        LegacyPort = new InMemoryCatalogLegacyPort(State);
        ProductRepository = new InMemoryProductRepository(State);
        CategoryRepository = new InMemoryCategoryRepository(State);
        StockMovementRepository = new InMemoryStockMovementRepository(State);
        StockLegacyPort = new InMemoryStockLegacyPort(State);
        ProductQueries = new InMemoryProductQueries(State);
        CategoryQueries = new InMemoryCategoryQueries(State);
    }

    public void Reset() => State.Reset();
}
