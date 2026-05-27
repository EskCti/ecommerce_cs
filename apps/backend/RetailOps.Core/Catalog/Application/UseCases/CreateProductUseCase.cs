using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed class CreateProductUseCase(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ProductRegistrationPolicy registrationPolicy) : IUseCase<(int tenantId, CreateProductInputDto input), ProductOutputDto>
{
    public async Task<Result<ProductOutputDto>> Execute(
        (int tenantId, CreateProductInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<ProductOutputDto>.Failure(tenantIdResult.Error);

        var barcodeResult = Barcode.Create(input.Barcode);
        if (barcodeResult.IsFailure)
            return Result<ProductOutputDto>.Failure(barcodeResult.Error);

        var policyResult = await registrationPolicy.ValidateUniqueBarcodeAsync(
            tenantIdResult.Value,
            barcodeResult.Value,
            cancellationToken: cancellationToken);
        if (policyResult.IsFailure)
            return Result<ProductOutputDto>.Failure(policyResult.Error);

        var categoryResult = await categoryRepository.GetById(input.CategoryId);
        if (categoryResult.IsFailure)
            return Result<ProductOutputDto>.Failure(categoryResult.Error);

        if (categoryResult.Value.TenantId.Value != tenantId)
            return Result<ProductOutputDto>.Failure("Category does not belong to this tenant.");

        var buildResult = BuildProduct(tenantIdResult.Value, input, barcodeResult.Value);
        if (buildResult.IsFailure)
            return Result<ProductOutputDto>.Failure(buildResult.Error);

        var saveResult = await productRepository.Save(buildResult.Value);
        if (saveResult.IsFailure)
            return Result<ProductOutputDto>.Failure(saveResult.Error);

        return Result<ProductOutputDto>.Success(ProductOutputDto.FromDomain(buildResult.Value));
    }

    internal static Result<Product> BuildProduct(TenantId tenantId, CreateProductInputDto input, Barcode barcode)
    {
        var nameResult = ProductName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<Product>.Failure(nameResult.Error);

        var salePriceResult = SalePrice.Create(input.SalePrice);
        if (salePriceResult.IsFailure)
            return Result<Product>.Failure(salePriceResult.Error);

        var costPriceResult = CostPrice.Create(input.CostPrice);
        if (costPriceResult.IsFailure)
            return Result<Product>.Failure(costPriceResult.Error);

        var stockResult = StockQuantity.Create(input.InitialStock);
        if (stockResult.IsFailure)
            return Result<Product>.Failure(stockResult.Error);

        var alertResult = StockAlertLevel.Create(input.StockAlertLevel);
        if (alertResult.IsFailure)
            return Result<Product>.Failure(alertResult.Error);

        var categoryIdResult = CategoryId.Create(input.CategoryId);
        if (categoryIdResult.IsFailure)
            return Result<Product>.Failure(categoryIdResult.Error);

        ProductPhotoPath? photoPath = null;
        if (!string.IsNullOrWhiteSpace(input.PhotoPath))
        {
            var photoResult = ProductPhotoPath.Create(input.PhotoPath);
            if (photoResult.IsFailure)
                return Result<Product>.Failure(photoResult.Error);
            photoPath = photoResult.Value;
        }

        return Product.Create(
            tenantId,
            barcode,
            nameResult.Value,
            salePriceResult.Value,
            costPriceResult.Value,
            stockResult.Value,
            alertResult.Value,
            categoryIdResult.Value,
            input.Description,
            input.SupplierId,
            photoPath,
            input.IsActive);
    }
}
