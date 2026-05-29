using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed class ProductCatalogService(
    IProductRepository productRepository,
    IStockLegacyPort stockLegacyPort,
    StockAdjustmentPolicy stockAdjustmentPolicy)
    : IProductCatalogService
{
    public async Task<Result<Domain.Entities.Product?>> FindByBarcodeAsync(
        TenantId tenantId,
        string barcode,
        CancellationToken ct = default)
    {
        var barcodeResult = Barcode.Create(barcode);
        if (barcodeResult.IsFailure)
            return Result<Domain.Entities.Product?>.Failure(barcodeResult.Error);

        var result = await productRepository.FindByBarcode(tenantId, barcodeResult.Value);
        return result.IsFailure
            ? Result<Domain.Entities.Product?>.Failure(result.Error)
            : Result<Domain.Entities.Product?>.Success(result.Value);
    }

    public async Task<Result<int>> GetAvailableStockAsync(
        TenantId tenantId,
        Guid productId,
        CancellationToken ct = default)
    {
        var productResult = await productRepository.GetById(productId);
        if (productResult.IsFailure)
            return Result<int>.Failure(productResult.Error);

        if (productResult.Value.TenantId.Value != tenantId.Value)
            return Result<int>.Failure("Product does not belong to this tenant.");

        var product = productResult.Value;
        if (product.GradeDimensions.Count > 0)
            return Result<int>.Failure("Use GetVariantStockAsync for graded products.");

        return Result<int>.Success(product.Stock.Value);
    }

    public async Task<Result<int>> GetVariantStockAsync(
        TenantId tenantId,
        Guid variantId,
        CancellationToken ct = default)
    {
        var lookup = await productRepository.FindVariantById(variantId);
        if (lookup.IsFailure)
            return Result<int>.Failure(lookup.Error);

        var (product, variant) = lookup.Value;
        if (product.TenantId.Value != tenantId.Value)
            return Result<int>.Failure("Product does not belong to this tenant.");

        return Result<int>.Success(variant.Stock.Value);
    }

    public async Task<Result> ReserveStockAsync(
        TenantId tenantId,
        Guid productId,
        int quantity,
        CancellationToken ct = default)
    {
        var productResult = await productRepository.GetById(productId);
        if (productResult.IsFailure)
            return Result.Failure(productResult.Error);

        var product = productResult.Value;
        if (product.TenantId.Value != tenantId.Value)
            return Result.Failure("Product does not belong to this tenant.");

        if (product.GradeDimensions.Count > 0)
            return Result.Failure("Use ReserveVariantStockAsync for graded products.");

        var exitQuantityResult = StockQuantity.Create(quantity);
        if (exitQuantityResult.IsFailure)
            return Result.Failure(exitQuantityResult.Error);

        var validation = stockAdjustmentPolicy.ValidateExit(product.Stock, exitQuantityResult.Value);
        if (validation.IsFailure)
            return validation;

        return await stockLegacyPort.ReserveStockAsync(tenantId, productId, quantity, ct);
    }

    public async Task<Result> ReserveVariantStockAsync(
        TenantId tenantId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default)
    {
        var lookup = await productRepository.FindVariantById(variantId);
        if (lookup.IsFailure)
            return Result.Failure(lookup.Error);

        var (product, variant) = lookup.Value;
        if (product.TenantId.Value != tenantId.Value)
            return Result.Failure("Product does not belong to this tenant.");

        var exitQuantityResult = StockQuantity.Create(quantity);
        if (exitQuantityResult.IsFailure)
            return Result.Failure(exitQuantityResult.Error);

        var validation = stockAdjustmentPolicy.ValidateExit(variant.Stock, exitQuantityResult.Value);
        if (validation.IsFailure)
            return validation;

        return await stockLegacyPort.ReserveVariantStockAsync(tenantId, variantId, quantity, ct);
    }

    public async Task<Result> ReleaseStockAsync(
        TenantId tenantId,
        Guid productId,
        int quantity,
        CancellationToken ct = default) =>
        await stockLegacyPort.ReleaseStockAsync(tenantId, productId, quantity, ct);

    public async Task<Result> ReleaseVariantStockAsync(
        TenantId tenantId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default) =>
        await stockLegacyPort.ReleaseVariantStockAsync(tenantId, variantId, quantity, ct);

    public async Task<Result<IReadOnlyList<GradeVariantOutputDto>>> FindVariantsByProductAsync(
        TenantId tenantId,
        Guid productId,
        CancellationToken ct = default)
    {
        var productResult = await productRepository.GetById(productId);
        if (productResult.IsFailure)
            return Result<IReadOnlyList<GradeVariantOutputDto>>.Failure(productResult.Error);

        var product = productResult.Value;
        if (product.TenantId.Value != tenantId.Value)
            return Result<IReadOnlyList<GradeVariantOutputDto>>.Failure("Product does not belong to this tenant.");

        var variants = product.GradeVariants
            .Select(v => GradeVariantOutputDto.FromDomain(v, product))
            .ToList();

        return Result<IReadOnlyList<GradeVariantOutputDto>>.Success(variants);
    }
}
