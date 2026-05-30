using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.Events;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed class PurchaseStockUseCase(
    IProductRepository productRepository,
    IStockMovementRepository stockMovementRepository,
    StockAdjustmentPolicy stockAdjustmentPolicy,
    IStockLegacyPort stockLegacyPort,
    IProductPurchasedPublisher productPurchasedPublisher)
    : IUseCase<(int tenantId, PurchaseStockInputDto input), StockMovementOutputDto>
{
    public async Task<Result<StockMovementOutputDto>> Execute(
        (int tenantId, PurchaseStockInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(tenantIdResult.Error);

        var productResult = await productRepository.GetById(input.ProductId);
        if (productResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(productResult.Error);

        var product = productResult.Value;
        if (product.TenantId.Value != tenantId)
            return Result<StockMovementOutputDto>.Failure("Product does not belong to this tenant.");

        var quantityResult = StockQuantity.Create(input.Quantity);
        if (quantityResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(quantityResult.Error);

        var costPriceResult = CostPrice.Create(input.UnitCost);
        if (costPriceResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(costPriceResult.Error);

        var newStockResult = StockQuantity.Create(product.Stock.Value + input.Quantity);
        if (newStockResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(newStockResult.Error);

        var adjustResult = product.AdjustStock(newStockResult.Value, stockAdjustmentPolicy);
        if (adjustResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(adjustResult.Error);

        var updateCost = product.UpdateDetails(costPrice: costPriceResult.Value);
        if (updateCost.IsFailure)
            return Result<StockMovementOutputDto>.Failure(updateCost.Error);

        var movementResult = StockMovement.Create(
            product.Id,
            quantityResult.Value,
            input.Reason,
            input.UserId,
            StockMovementType.Purchase);

        if (movementResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(movementResult.Error);

        var legacyAdjust = await stockLegacyPort.AdjustStockAsync(
            tenantIdResult.Value,
            product.Id,
            newStockResult.Value.Value,
            cancellationToken);

        if (legacyAdjust.IsFailure)
            return Result<StockMovementOutputDto>.Failure(legacyAdjust.Error);

        var saveProduct = await productRepository.Save(product);
        if (saveProduct.IsFailure)
            return Result<StockMovementOutputDto>.Failure(saveProduct.Error);

        var saveMovement = await stockMovementRepository.Save(movementResult.Value);
        if (saveMovement.IsFailure)
            return Result<StockMovementOutputDto>.Failure(saveMovement.Error);

        try
        {
            await productPurchasedPublisher.PublishAsync(
                new ProductPurchased(
                    product.Id,
                    tenantId,
                    input.Quantity,
                    input.UnitCost,
                    DateTime.UtcNow,
                    input.DueDate ?? DateTime.UtcNow,
                    product.Name.Value,
                    input.Reason.Trim(),
                    input.SupplierLegacyId),
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Result<StockMovementOutputDto>.Failure(ex.Message);
        }

        return Result<StockMovementOutputDto>.Success(StockMovementOutputDto.FromDomain(movementResult.Value));
    }
}

public sealed class ProductPurchasedPublisherStub : IProductPurchasedPublisher
{
    public Task PublishAsync(ProductPurchased @event, CancellationToken ct = default) => Task.CompletedTask;
}
