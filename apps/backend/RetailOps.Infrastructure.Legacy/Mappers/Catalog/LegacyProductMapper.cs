using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Catalog;

public static class LegacyProductMapper
{
    public static Result<Product> ToDomain(
        LegacyProductRow row,
        IEnumerable<GradeDimension>? gradeDimensions = null)
    {
        try
        {
            var tenantIdResult = TenantId.Create(row.CompanyId);
            if (tenantIdResult.IsFailure)
                return Result<Product>.Failure(tenantIdResult.Error);

            if (string.IsNullOrWhiteSpace(row.Code))
                return Result<Product>.Failure("Product barcode is required.");

            if (string.IsNullOrWhiteSpace(row.Name))
                return Result<Product>.Failure("Product name is required.");

            var barcodeResult = Barcode.Create(row.Code);
            if (barcodeResult.IsFailure)
                return Result<Product>.Failure(barcodeResult.Error);

            var nameResult = ProductName.Create(row.Name);
            if (nameResult.IsFailure)
                return Result<Product>.Failure(nameResult.Error);

            var salePriceResult = SalePrice.Create(row.SalePrice);
            if (salePriceResult.IsFailure)
                return Result<Product>.Failure(salePriceResult.Error);

            var costPriceResult = CostPrice.Create(row.CostPrice);
            if (costPriceResult.IsFailure)
                return Result<Product>.Failure(costPriceResult.Error);

            var stockResult = StockQuantity.Create(row.Stock);
            if (stockResult.IsFailure)
                return Result<Product>.Failure(stockResult.Error);

            var alertResult = StockAlertLevel.Create(row.StockAlertLevel);
            if (alertResult.IsFailure)
                return Result<Product>.Failure(alertResult.Error);

            if (row.CategoryLegacyId is null or <= 0)
                return Result<Product>.Failure("Product category is required.");

            var categoryIdResult = CategoryId.Create(LegacyCatalogIds.Category(row.CategoryLegacyId.Value));
            if (categoryIdResult.IsFailure)
                return Result<Product>.Failure(categoryIdResult.Error);

            var marginResult = ProfitMargin.Create(row.ProfitMargin);
            if (marginResult.IsFailure)
                return Result<Product>.Failure(marginResult.Error);

            ProductPhotoPath? photoPath = null;
            if (!string.IsNullOrWhiteSpace(row.Photo))
            {
                var photoResult = ProductPhotoPath.Create(row.Photo);
                if (photoResult.IsFailure)
                    return Result<Product>.Failure(photoResult.Error);
                photoPath = photoResult.Value;
            }

            var isActive = row.Active is null
                || row.Active.Equals("S", StringComparison.OrdinalIgnoreCase)
                || row.Active.Equals("Sim", StringComparison.OrdinalIgnoreCase);

            Guid? supplierId = row.SupplierLegacyId is > 0
                ? Mappers.Crm.LegacyCrmIds.Supplier(row.SupplierLegacyId.Value)
                : null;

            return Product.Reconstitute(
                LegacyCatalogIds.Product(row.Id),
                tenantIdResult.Value,
                barcodeResult.Value,
                nameResult.Value,
                row.Description,
                salePriceResult.Value,
                costPriceResult.Value,
                stockResult.Value,
                marginResult.Value,
                alertResult.Value,
                categoryIdResult.Value,
                supplierId,
                photoPath,
                isActive,
                salePriceResult.Value.IsOpenPrice,
                DateTime.UtcNow,
                DateTime.UtcNow,
                gradeDimensions);
        }
        catch (Exception ex)
        {
            return Result<Product>.Failure($"Failed to map legacy Product: {ex.Message}");
        }
    }

    public static Result<LegacyProductRow> ToLegacy(Product product, int? legacyId = null)
    {
        try
        {
            var categoryLegacyId = LegacyCatalogIds.ParseCategoryLegacyId(product.CategoryId.Value);
            var supplierLegacyId = product.SupplierId is not null
                ? Mappers.Crm.LegacyCrmIds.ParseLegacyId(product.SupplierId.Value, "0007")
                : null;

            var row = new LegacyProductRow
            {
                Id = legacyId ?? LegacyCatalogIds.ParseLegacyId(product.Id, "0009") ?? 0,
                CompanyId = product.TenantId.Value,
                Code = product.Barcode.Value,
                Name = product.Name.Value,
                Description = product.Description,
                Stock = product.Stock.Value,
                SalePrice = product.SalePrice.Value,
                CostPrice = product.CostPrice.Value,
                ProfitMargin = product.ProfitMargin.Value,
                SupplierLegacyId = supplierLegacyId,
                CategoryLegacyId = categoryLegacyId,
                StockAlertLevel = product.StockAlertLevel.Value,
                Active = product.IsActive ? "Sim" : "Não",
                Photo = product.PhotoPath?.Value ?? string.Empty
            };

            return Result<LegacyProductRow>.Success(row);
        }
        catch (Exception ex)
        {
            return Result<LegacyProductRow>.Failure($"Failed to map Product to legacy: {ex.Message}");
        }
    }
}
