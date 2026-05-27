using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.Ports;

public interface ICatalogLegacyPort
{
    Task<Result<IReadOnlyList<Product>>> GetProductsFromLegacyAsync(TenantId tenantId, CancellationToken ct = default);
    Task<Result<Product?>> GetProductFromLegacyAsync(TenantId tenantId, Guid productId, CancellationToken ct = default);
    Task<Result<Product?>> GetProductByBarcodeFromLegacyAsync(TenantId tenantId, string barcode, CancellationToken ct = default);
    Task<Result<int>> SaveProductToLegacyAsync(Product product, CancellationToken ct = default);

    Task<Result<IReadOnlyList<Category>>> GetCategoriesFromLegacyAsync(TenantId tenantId, CancellationToken ct = default);
    Task<Result<Category?>> GetCategoryFromLegacyAsync(TenantId tenantId, Guid categoryId, CancellationToken ct = default);
    Task<Result<int>> SaveCategoryToLegacyAsync(Category category, CancellationToken ct = default);

    Task<Result<int>> SaveStockMovementToLegacyAsync(StockMovement movement, TenantId tenantId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<StockMovement>>> GetStockMovementsFromLegacyAsync(Guid productId, CancellationToken ct = default);
}

public interface IStockLegacyPort
{
    Task<Result> AdjustStockAsync(TenantId tenantId, Guid productId, int newQuantity, CancellationToken ct = default);
    Task<Result> ReserveStockAsync(TenantId tenantId, Guid productId, int quantity, CancellationToken ct = default);
    Task<Result> ReleaseStockAsync(TenantId tenantId, Guid productId, int quantity, CancellationToken ct = default);
}

public interface IProductCatalogService
{
    Task<Result<Product?>> FindByBarcodeAsync(TenantId tenantId, string barcode, CancellationToken ct = default);
    Task<Result<int>> GetAvailableStockAsync(TenantId tenantId, Guid productId, CancellationToken ct = default);
    Task<Result> ReserveStockAsync(TenantId tenantId, Guid productId, int quantity, CancellationToken ct = default);
    Task<Result> ReleaseStockAsync(TenantId tenantId, Guid productId, int quantity, CancellationToken ct = default);
}
