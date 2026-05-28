using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Events;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Core.Sales.Domain.Services;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class CancelSaleUseCase(
    ISaleRepository saleRepository,
    ISalesLegacyPort salesLegacyPort,
    CartStockReservationService stockReservation) : IUseCase<(int tenantId, Guid saleId), SaleOutputDto>
{
    public async Task<Result<SaleOutputDto>> Execute(
        (int tenantId, Guid saleId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, saleId) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<SaleOutputDto>.Failure(tenantIdResult.Error);

        var saleResult = await saleRepository.GetById(saleId);
        if (saleResult.IsFailure)
            return Result<SaleOutputDto>.Failure(saleResult.Error);

        if (saleResult.Value.TenantId.Value != tenantId)
            return Result<SaleOutputDto>.Failure("Sale does not belong to this tenant.");

        var sale = saleResult.Value;
        var cancelResult = sale.Cancel();
        if (cancelResult.IsFailure)
            return Result<SaleOutputDto>.Failure(cancelResult.Error);

        var release = await stockReservation.ReleaseLinesAsync(tenantIdResult.Value, sale.Lines, cancellationToken);
        if (release.IsFailure)
            return Result<SaleOutputDto>.Failure(release.Error);

        var legacyCancel = await salesLegacyPort.CancelSale(sale, cancellationToken);
        if (legacyCancel.IsFailure)
            return Result<SaleOutputDto>.Failure(legacyCancel.Error);

        var saveResult = await saleRepository.Save(sale);
        if (saveResult.IsFailure)
            return Result<SaleOutputDto>.Failure(saveResult.Error);

        _ = new SaleCancelledEvent(sale.Id, tenantId, sale.CancelledAt!.Value);

        return Result<SaleOutputDto>.Success(SaleOutputDto.FromDomain(sale));
    }
}
