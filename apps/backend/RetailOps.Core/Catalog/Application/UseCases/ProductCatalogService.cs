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

        return Result<int>.Success(productResult.Value.Stock.Value);
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

        var exitQuantityResult = StockQuantity.Create(quantity);
        if (exitQuantityResult.IsFailure)
            return Result.Failure(exitQuantityResult.Error);

        var validation = stockAdjustmentPolicy.ValidateExit(product.Stock, exitQuantityResult.Value);
        if (validation.IsFailure)
            return validation;

        return await stockLegacyPort.ReserveStockAsync(tenantId, productId, quantity, ct);
    }

    public async Task<Result> ReleaseStockAsync(
        TenantId tenantId,
        Guid productId,
        int quantity,
        CancellationToken ct = default) =>
        await stockLegacyPort.ReleaseStockAsync(tenantId, productId, quantity, ct);
}
