using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class GetPaymentMethodUseCase : IUseCase<(int tenantId, Guid paymentMethodId), PaymentMethodOutputDto>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public GetPaymentMethodUseCase(IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<Result<PaymentMethodOutputDto>> Execute(
        (int tenantId, Guid paymentMethodId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, paymentMethodId) = request;

        var tenantIdResult = RetailOps.Shared.Kernel.Domain.ValueObjects.TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(tenantIdResult.Error);

        var paymentMethodResult = await _paymentMethodRepository.GetById(paymentMethodId);
        if (paymentMethodResult.IsFailure)
            return Result<PaymentMethodOutputDto>.Failure(paymentMethodResult.Error);

        var paymentMethod = paymentMethodResult.Value;

        // Verify tenant ownership
        if (paymentMethod.TenantId.Value != tenantId)
            return Result<PaymentMethodOutputDto>.Failure("Payment method does not belong to this tenant.");

        var outputDto = PaymentMethodOutputDto.FromDomain(paymentMethod);
        return Result<PaymentMethodOutputDto>.Success(outputDto);
    }
}