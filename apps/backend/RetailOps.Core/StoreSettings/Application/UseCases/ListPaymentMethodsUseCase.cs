using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class ListPaymentMethodsUseCase : IUseCase<int, IEnumerable<PaymentMethodOutputDto>>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public ListPaymentMethodsUseCase(IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<Result<IEnumerable<PaymentMethodOutputDto>>> Execute(
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenantIdResult = RetailOps.Shared.Kernel.Domain.ValueObjects.TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<IEnumerable<PaymentMethodOutputDto>>.Failure(tenantIdResult.Error);

        var paymentMethodsResult = await _paymentMethodRepository.GetByTenantId(tenantIdResult.Value);
        if (paymentMethodsResult.IsFailure)
            return Result<IEnumerable<PaymentMethodOutputDto>>.Failure(paymentMethodsResult.Error);

        var paymentMethods = paymentMethodsResult.Value;
        var outputDtos = paymentMethods.Select(PaymentMethodOutputDto.FromDomain);

        return Result<IEnumerable<PaymentMethodOutputDto>>.Success(outputDtos);
    }
}