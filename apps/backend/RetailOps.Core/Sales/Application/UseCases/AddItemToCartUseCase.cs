using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Core.Sales.Domain.Services;
using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class AddItemToCartUseCase(
    ICashSessionRepository cashSessionRepository,
    IProductCatalogService productCatalogService,
    ISalesLegacyPort salesLegacyPort,
    CartStockReservationService stockReservation) : IUseCase<(int tenantId, Guid operatorUserId, AddItemToCartInputDto input), CashSessionOutputDto>
{
    public async Task<Result<CashSessionOutputDto>> Execute(
        (int tenantId, Guid operatorUserId, AddItemToCartInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, operatorUserId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(tenantIdResult.Error);

        var sessionResult = await cashSessionRepository.GetOpenByOperator(tenantIdResult.Value, operatorUserId);
        if (sessionResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(sessionResult.Error);

        if (sessionResult.Value is null)
            return Result<CashSessionOutputDto>.Failure("No open cash session found.");

        var scanResult = ScanQuantityPrefix.Parse(input.ScannedValue);
        if (scanResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(scanResult.Error);

        var productResult = await productCatalogService.FindByBarcodeAsync(
            tenantIdResult.Value,
            scanResult.Value.Barcode,
            cancellationToken);

        if (productResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(productResult.Error);

        if (productResult.Value is null)
            return Result<CashSessionOutputDto>.Failure("Product not found.");

        var product = productResult.Value;
        var quantity = scanResult.Value.Quantity;

        if (!product.IsOpenPrice)
        {
            var stockResult = await productCatalogService.GetAvailableStockAsync(
                tenantIdResult.Value,
                product.Id,
                cancellationToken);

            if (stockResult.IsFailure)
                return Result<CashSessionOutputDto>.Failure(stockResult.Error);

            if (stockResult.Value < quantity)
                return Result<CashSessionOutputDto>.Failure("Insufficient stock for requested quantity.");
        }

        var unitPriceValue = input.UnitPriceOverride ?? product.SalePrice.Value;
        var unitPriceResult = UnitPrice.Create(unitPriceValue);
        if (unitPriceResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(unitPriceResult.Error);

        var requiresGrade = product.GradeDimensions.Count > 0;
        var lineResult = SaleLine.Create(
            product.Id,
            product.Barcode.Value,
            quantity,
            unitPriceResult.Value,
            requiresGrade);

        if (lineResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(lineResult.Error);

        var session = sessionResult.Value;
        var addResult = session.AddLine(lineResult.Value);
        if (addResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(addResult.Error);

        if (lineResult.Value.Status == SaleLineStatus.Ready)
        {
            var reserve = await stockReservation.ReserveLineAsync(
                tenantIdResult.Value,
                lineResult.Value,
                cancellationToken);

            if (reserve.IsFailure)
            {
                session.RemoveLine(lineResult.Value.Id);
                return Result<CashSessionOutputDto>.Failure(reserve.Error);
            }
        }

        var legacyAdd = await salesLegacyPort.AddCartLine(session, lineResult.Value, cancellationToken);
        if (legacyAdd.IsFailure)
            return Result<CashSessionOutputDto>.Failure(legacyAdd.Error);

        var saveResult = await cashSessionRepository.Save(session);
        if (saveResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(saveResult.Error);

        return Result<CashSessionOutputDto>.Success(CashSessionOutputDto.FromDomain(session));
    }
}
