using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Core.Returns.Application.DTOs;
using RetailOps.Core.Returns.Application.Ports;
using RetailOps.Core.Returns.Domain.Entities;
using RetailOps.Core.Returns.Domain.Events;
using RetailOps.Core.Returns.Domain.Repositories;
using RetailOps.Core.Returns.Domain.Services;
using RetailOps.Core.Returns.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Returns.Application.UseCases;

public sealed class RegisterExchangeUseCase(
    IExchangeRepository exchangeRepository,
    ExchangeStockPolicy exchangeStockPolicy,
    ICustomerProvisioningPort customerProvisioningPort)
    : IUseCase<(int tenantId, Guid operatorUserId, RegisterExchangeInputDto input), ExchangeOutputDto>
{
    public ProductExchanged? LastEvent { get; private set; }

    public async Task<Result<ExchangeOutputDto>> Execute(
        (int tenantId, Guid operatorUserId, RegisterExchangeInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, operatorUserId, input) = request;
        LastEvent = null;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<ExchangeOutputDto>.Failure(tenantIdResult.Error);

        var gradeInResult = GradeSelection.Create(input.GradeVariantInId, input.GradeOptionIdsIn);
        if (gradeInResult.IsFailure)
            return Result<ExchangeOutputDto>.Failure(gradeInResult.Error);

        var gradeOutResult = GradeSelection.Create(input.GradeVariantOutId, input.GradeOptionIdsOut);
        if (gradeOutResult.IsFailure)
            return Result<ExchangeOutputDto>.Failure(gradeOutResult.Error);

        var customerIdResult = await ResolveCustomerIdAsync(
            tenantIdResult.Value,
            input,
            cancellationToken);
        if (customerIdResult.IsFailure)
            return Result<ExchangeOutputDto>.Failure(customerIdResult.Error);

        var stockResult = await exchangeStockPolicy.GetAvailableOutboundStockAsync(
            tenantIdResult.Value,
            input.ProductOutId,
            gradeOutResult.Value,
            cancellationToken);
        if (stockResult.IsFailure)
            return Result<ExchangeOutputDto>.Failure(stockResult.Error);

        var exchangeResult = Exchange.Register(
            tenantIdResult.Value,
            customerIdResult.Value,
            input.ProductInId,
            input.ProductOutId,
            gradeInResult.Value,
            gradeOutResult.Value,
            operatorUserId,
            stockResult.Value);
        if (exchangeResult.IsFailure)
            return Result<ExchangeOutputDto>.Failure(exchangeResult.Error);

        var stockAdjust = await exchangeStockPolicy.ApplyRegisterAsync(
            tenantIdResult.Value,
            input.ProductInId,
            gradeInResult.Value,
            input.ProductOutId,
            gradeOutResult.Value,
            cancellationToken);
        if (stockAdjust.IsFailure)
            return Result<ExchangeOutputDto>.Failure(stockAdjust.Error);

        var saveResult = await exchangeRepository.Save(exchangeResult.Value, cancellationToken);
        if (saveResult.IsFailure)
        {
            await exchangeStockPolicy.ApplyDeleteReverseAsync(
                tenantIdResult.Value,
                input.ProductInId,
                gradeInResult.Value,
                input.ProductOutId,
                gradeOutResult.Value,
                cancellationToken);
            return Result<ExchangeOutputDto>.Failure(saveResult.Error);
        }

        LastEvent = new ProductExchanged(
            exchangeResult.Value.Id,
            tenantId,
            customerIdResult.Value.Value,
            input.ProductInId,
            input.ProductOutId,
            exchangeResult.Value.RegisteredAt);

        return Result<ExchangeOutputDto>.Success(ExchangeOutputDto.FromDomain(exchangeResult.Value));
    }

    private async Task<Result<CustomerId>> ResolveCustomerIdAsync(
        TenantId tenantId,
        RegisterExchangeInputDto input,
        CancellationToken ct)
    {
        if (input.CustomerId is Guid customerId && customerId != Guid.Empty)
            return CustomerId.Create(customerId);

        if (string.IsNullOrWhiteSpace(input.Cpf))
            return Result<CustomerId>.Failure("Customer id or CPF is required.");

        if (string.IsNullOrWhiteSpace(input.CustomerName))
            return Result<CustomerId>.Failure("Customer name is required when creating by CPF.");

        var provisioned = await customerProvisioningPort.FindOrCreateByCpfAsync(
            tenantId,
            input.Cpf,
            input.CustomerName,
            ct);
        if (provisioned.IsFailure)
            return Result<CustomerId>.Failure(provisioned.Error);

        return CustomerId.Create(provisioned.Value);
    }
}
