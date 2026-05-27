using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed class UpdateProductUseCase(IProductRepository productRepository)
    : IUseCase<(int tenantId, Guid productId, UpdateProductInputDto input), ProductOutputDto>
{
    public async Task<Result<ProductOutputDto>> Execute(
        (int tenantId, Guid productId, UpdateProductInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, productId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<ProductOutputDto>.Failure(tenantIdResult.Error);

        var productResult = await productRepository.GetById(productId);
        if (productResult.IsFailure)
            return Result<ProductOutputDto>.Failure(productResult.Error);

        var product = productResult.Value;
        if (product.TenantId.Value != tenantId)
            return Result<ProductOutputDto>.Failure("Product does not belong to this tenant.");

        ProductName? name = null;
        if (input.Name is not null)
        {
            var nameResult = ProductName.Create(input.Name);
            if (nameResult.IsFailure)
                return Result<ProductOutputDto>.Failure(nameResult.Error);
            name = nameResult.Value;
        }

        SalePrice? salePrice = null;
        if (input.SalePrice.HasValue)
        {
            var salePriceResult = SalePrice.Create(input.SalePrice.Value);
            if (salePriceResult.IsFailure)
                return Result<ProductOutputDto>.Failure(salePriceResult.Error);
            salePrice = salePriceResult.Value;
        }

        CostPrice? costPrice = null;
        if (input.CostPrice.HasValue)
        {
            var costPriceResult = CostPrice.Create(input.CostPrice.Value);
            if (costPriceResult.IsFailure)
                return Result<ProductOutputDto>.Failure(costPriceResult.Error);
            costPrice = costPriceResult.Value;
        }

        StockAlertLevel? alertLevel = null;
        if (input.StockAlertLevel.HasValue)
        {
            var alertResult = StockAlertLevel.Create(input.StockAlertLevel.Value);
            if (alertResult.IsFailure)
                return Result<ProductOutputDto>.Failure(alertResult.Error);
            alertLevel = alertResult.Value;
        }

        CategoryId? categoryId = null;
        if (input.CategoryId.HasValue)
        {
            var categoryIdResult = CategoryId.Create(input.CategoryId.Value);
            if (categoryIdResult.IsFailure)
                return Result<ProductOutputDto>.Failure(categoryIdResult.Error);
            categoryId = categoryIdResult.Value;
        }

        ProductPhotoPath? photoPath = null;
        var updatePhoto = input.PhotoPath is not null;
        if (updatePhoto)
        {
            var photoResult = ProductPhotoPath.Create(input.PhotoPath!);
            if (photoResult.IsFailure)
                return Result<ProductOutputDto>.Failure(photoResult.Error);
            photoPath = photoResult.Value;
        }

        var updateResult = product.UpdateDetails(
            name,
            input.Description,
            salePrice,
            costPrice,
            alertLevel,
            categoryId,
            input.SupplierId,
            photoPath,
            updatePhoto,
            input.ClearSupplier);

        if (updateResult.IsFailure)
            return Result<ProductOutputDto>.Failure(updateResult.Error);

        if (input.IsActive.HasValue)
        {
            var statusResult = input.IsActive.Value ? product.Activate() : product.Deactivate();
            if (statusResult.IsFailure)
                return Result<ProductOutputDto>.Failure(statusResult.Error);
        }

        var saveResult = await productRepository.Save(product);
        if (saveResult.IsFailure)
            return Result<ProductOutputDto>.Failure(saveResult.Error);

        return Result<ProductOutputDto>.Success(ProductOutputDto.FromDomain(product));
    }
}
