using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Events;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Core.Sales.Domain.Services;
using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class FinalizeSaleUseCase(
    ICashSessionRepository cashSessionRepository,
    ISaleRepository saleRepository,
    IStoreConfigRepository storeConfigRepository,
    ISalesLegacyPort salesLegacyPort,
    ISaleCompletedPublisher saleCompletedPublisher,
    ISaleParallelRunLogger parallelRunLogger) : IUseCase<(int tenantId, Guid operatorUserId, FinalizeSaleInputDto input), SaleOutputDto>
{
    public async Task<Result<SaleOutputDto>> Execute(
        (int tenantId, Guid operatorUserId, FinalizeSaleInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, operatorUserId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<SaleOutputDto>.Failure(tenantIdResult.Error);

        var sessionResult = await cashSessionRepository.GetOpenByOperator(tenantIdResult.Value, operatorUserId);
        if (sessionResult.IsFailure)
            return Result<SaleOutputDto>.Failure(sessionResult.Error);

        if (sessionResult.Value is null)
            return Result<SaleOutputDto>.Failure("No open cash session found.");

        if (!Enum.TryParse<PaymentTerms>(input.PaymentTerms, true, out var paymentTerms))
            return Result<SaleOutputDto>.Failure($"Invalid payment terms: {input.PaymentTerms}");

        var discountResult = Discount.Create(input.DiscountAmount);
        if (discountResult.IsFailure)
            return Result<SaleOutputDto>.Failure(discountResult.Error);

        var session = sessionResult.Value;
        var subtotal = session.Lines
            .Where(l => l.Status == SaleLineStatus.Ready)
            .Sum(l => l.CalculateTotal().Value);

        var totalAfterDiscount = Math.Max(0, subtotal - discountResult.Value.Value);

        var policyResult = SaleFinalizationPolicy.Validate(
            session,
            paymentTerms,
            input.CustomerId,
            input.AmountPaid,
            totalAfterDiscount);

        if (policyResult.IsFailure)
            return Result<SaleOutputDto>.Failure(policyResult.Error);

        var changeResult = ChangeAmount.Create(input.AmountPaid - totalAfterDiscount);
        if (changeResult.IsFailure)
            return Result<SaleOutputDto>.Failure(changeResult.Error);

        var configResult = await storeConfigRepository.GetByTenantId(tenantIdResult.Value);
        if (configResult.IsFailure)
            return Result<SaleOutputDto>.Failure(configResult.Error);

        var commissionPercent = input.SellerCommissionPercent ?? configResult.Value.CommissionRate.Value;
        var commissionResult = CommissionCalculator.Calculate(totalAfterDiscount, commissionPercent);
        if (commissionResult.IsFailure)
            return Result<SaleOutputDto>.Failure(commissionResult.Error);

        var readyLines = session.Lines.Where(l => l.Status == SaleLineStatus.Ready).ToList();
        var saleResult = Sale.Create(
            tenantIdResult.Value,
            session.Id,
            operatorUserId,
            paymentTerms,
            input.CustomerId,
            input.PaymentMethodId,
            subtotal,
            discountResult.Value,
            totalAfterDiscount,
            changeResult.Value,
            commissionResult.Value,
            readyLines);

        if (saleResult.IsFailure)
            return Result<SaleOutputDto>.Failure(saleResult.Error);

        var sale = saleResult.Value;
        var legacyFinalize = await salesLegacyPort.FinalizeSale(session, sale, cancellationToken);
        if (legacyFinalize.IsFailure)
            return Result<SaleOutputDto>.Failure(legacyFinalize.Error);

        parallelRunLogger.LogComparison(
            tenantId,
            session.Id,
            sale.Id,
            sale.Total,
            sale.Total);

        session.RecordSaleTotal(sale.Total);
        foreach (var line in readyLines)
            session.RemoveLine(line.Id);

        var saveSession = await cashSessionRepository.Save(session);
        if (saveSession.IsFailure)
            return Result<SaleOutputDto>.Failure(saveSession.Error);

        var saveSale = await saleRepository.Save(sale);
        if (saveSale.IsFailure)
            return Result<SaleOutputDto>.Failure(saveSale.Error);

        var completedEvent = new SaleCompletedEvent(
            sale.Id,
            tenantId,
            session.Id,
            operatorUserId,
            sale.Total,
            sale.CommissionAmount,
            sale.CompletedAt);

        await saleCompletedPublisher.Publish(completedEvent, cancellationToken);

        return Result<SaleOutputDto>.Success(SaleOutputDto.FromDomain(sale));
    }
}
