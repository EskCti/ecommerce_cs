using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class CreatePaymentMethodUseCase : IUseCase<(int tenantId, CreatePaymentMethodInputDto input), PaymentMethodOutputDto>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public CreatePaymentMethodUseCase(IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<Result<PaymentMethodOutputDto>> Execute(
        (int tenantId, CreatePaymentMethodInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(tenantIdResult.Error);

        var nameResult = PaymentMethodName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(nameResult.Error);

        var surchargeResult = SurchargePercent.Create(input.SurchargePercent);
        if (surchargeResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(surchargeResult.Error);

        // Check if name already exists for this tenant
        var nameExistsResult = await _paymentMethodRepository.NameExists(tenantIdResult.Value, nameResult.Value);
        if (nameExistsResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(nameExistsResult.Error);

        if (nameExistsResult.Value)
            return Result<PaymentMethodOutputDto>.Failure($"Payment method name '{input.Name}' already exists for this tenant.");

        var paymentMethodResult = PaymentMethod.Create(
            tenantIdResult.Value,
            nameResult.Value,
            surchargeResult.Value,
            input.IsActive);

        if (paymentMethodResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(paymentMethodResult.Error);

        var paymentMethod = paymentMethodResult.Value;
        var saveResult = await _paymentMethodRepository.Save(paymentMethod);
        
        if (saveResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(saveResult.Error);

        var outputDto = PaymentMethodOutputDto.FromDomain(paymentMethod);
        return Result<PaymentMethodOutputDto>.Success(outputDto);
    }
}