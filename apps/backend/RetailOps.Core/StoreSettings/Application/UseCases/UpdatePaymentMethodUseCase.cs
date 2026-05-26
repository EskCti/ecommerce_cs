using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class UpdatePaymentMethodUseCase : IUseCase<(int tenantId, Guid paymentMethodId, UpdatePaymentMethodInputDto input), PaymentMethodOutputDto>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public UpdatePaymentMethodUseCase(IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<Result<PaymentMethodOutputDto>> Execute(
        (int tenantId, Guid paymentMethodId, UpdatePaymentMethodInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, paymentMethodId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(tenantIdResult.Error);

        var paymentMethodResult = await _paymentMethodRepository.GetById(paymentMethodId);
        if (paymentMethodResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(paymentMethodResult.Error);

        var paymentMethod = paymentMethodResult.Value;

        // Verify tenant ownership
        if (paymentMethod.TenantId.Value != tenantId)
            return Result<PaymentMethodOutputDto>.Failure("Payment method does not belong to this tenant.");

        // Update name if provided
        if (input.Name != null)
        {
            var nameResult = PaymentMethodName.Create(input.Name);
            if (nameResult.IsFailure)
                return Result<PaymentMethodOutputDto>.Failure(nameResult.Error);

            // Check if new name already exists (excluding current payment method)
            var nameExistsResult = await _paymentMethodRepository.NameExists(tenantIdResult.Value, nameResult.Value, paymentMethodId);
            if (nameExistsResult.IsFailure)
                return Result<PaymentMethodOutputDto>.Failure(nameExistsResult.Error);

            if (nameExistsResult.Value)
                return Result<PaymentMethodOutputDto>.Failure($"Payment method name '{input.Name}' already exists for this tenant.");

            var renameResult = paymentMethod.Rename(nameResult.Value);
            if (renameResult.IsFailure)
                return Result<PaymentMethodOutputDto>.Failure(renameResult.Error);
        }

        // Update surcharge if provided
        if (input.SurchargePercent.HasValue)
        {
            var surchargeResult = SurchargePercent.Create(input.SurchargePercent.Value);
            if (surchargeResult.IsFailure)
                return Result<PaymentMethodOutputDto>.Failure(surchargeResult.Error);

            var updateSurchargeResult = paymentMethod.UpdateSurcharge(surchargeResult.Value);
            if (updateSurchargeResult.IsFailure)
                return Result<PaymentMethodOutputDto>.Failure(updateSurchargeResult.Error);
        }

        // Update active status if provided
        if (input.IsActive.HasValue)
        {
            var statusResult = input.IsActive.Value 
                ? paymentMethod.Activate() 
                : paymentMethod.Deactivate();

            if (statusResult.IsFailure)
                return Result<PaymentMethodOutputDto>.Failure(statusResult.Error);
        }

        var saveResult = await _paymentMethodRepository.Save(paymentMethod);
        if (saveResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(saveResult.Error);

        var outputDto = PaymentMethodOutputDto.FromDomain(paymentMethod);
        return Result<PaymentMethodOutputDto>.Success(outputDto);
    }
}