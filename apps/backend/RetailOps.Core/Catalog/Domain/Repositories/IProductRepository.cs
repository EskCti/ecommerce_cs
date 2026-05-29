using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Result<Product?>> FindByBarcode(TenantId tenantId, Barcode barcode);
    Task<Result<bool>> ExistsBarcode(TenantId tenantId, Barcode barcode, Guid? excludeProductId = null);
    Task<Result<IEnumerable<Product>>> GetByTenantId(TenantId tenantId);
    Task<Result<(Product Product, GradeVariant Variant)>> FindVariantById(Guid variantId);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<Result<IEnumerable<Category>>> GetByTenantId(TenantId tenantId);
}

public interface IStockMovementRepository
{
    Task<Result> Save(StockMovement movement);
    Task<Result<IReadOnlyList<StockMovement>>> GetByProductId(Guid productId);
}
