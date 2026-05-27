using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed class RecordStockEntryUseCase(
    IProductRepository productRepository,
    IStockMovementRepository stockMovementRepository,
    StockAdjustmentPolicy stockAdjustmentPolicy,
    IStockLegacyPort stockLegacyPort)
    : IUseCase<(int tenantId, RecordStockMovementInputDto input), StockMovementOutputDto>
{
    public async Task<Result<StockMovementOutputDto>> Execute(
        (int tenantId, RecordStockMovementInputDto input) request,
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

        var newStockResult = StockQuantity.Create(product.Stock.Value + input.Quantity);
        if (newStockResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(newStockResult.Error);

        var adjustResult = product.AdjustStock(newStockResult.Value, stockAdjustmentPolicy);
        if (adjustResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(adjustResult.Error);

        var movementResult = StockMovement.Create(
            product.Id,
            quantityResult.Value,
            input.Reason,
            input.UserId,
            StockMovementType.Entry);

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

        return Result<StockMovementOutputDto>.Success(StockMovementOutputDto.FromDomain(movementResult.Value));
    }
}

public sealed class RecordStockExitUseCase(
    IProductRepository productRepository,
    IStockMovementRepository stockMovementRepository,
    StockAdjustmentPolicy stockAdjustmentPolicy,
    IStockLegacyPort stockLegacyPort)
    : IUseCase<(int tenantId, RecordStockMovementInputDto input), StockMovementOutputDto>
{
    public async Task<Result<StockMovementOutputDto>> Execute(
        (int tenantId, RecordStockMovementInputDto input) request,
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

        var exitQuantityResult = StockQuantity.Create(input.Quantity);
        if (exitQuantityResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(exitQuantityResult.Error);

        var validation = stockAdjustmentPolicy.ValidateExit(product.Stock, exitQuantityResult.Value);
        if (validation.IsFailure)
            return Result<StockMovementOutputDto>.Failure(validation.Error);

        var newStockResult = StockQuantity.Create(product.Stock.Value - input.Quantity);
        if (newStockResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(newStockResult.Error);

        var adjustResult = product.AdjustStock(newStockResult.Value, stockAdjustmentPolicy);
        if (adjustResult.IsFailure)
            return Result<StockMovementOutputDto>.Failure(adjustResult.Error);

        var movementResult = StockMovement.Create(
            product.Id,
            exitQuantityResult.Value,
            input.Reason,
            input.UserId,
            StockMovementType.Exit);

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

        return Result<StockMovementOutputDto>.Success(StockMovementOutputDto.FromDomain(movementResult.Value));
    }
}
