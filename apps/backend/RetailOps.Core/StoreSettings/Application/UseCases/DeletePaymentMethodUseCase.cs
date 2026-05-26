using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Repositories;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class DeletePaymentMethodUseCase : IUseCase<(int tenantId, Guid paymentMethodId), bool>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public DeletePaymentMethodUseCase(IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<Result<bool>> Execute(
        (int tenantId, Guid paymentMethodId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, paymentMethodId) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<bool>.Failure(tenantIdResult.Error);

        var paymentMethodResult = await _paymentMethodRepository.GetById(paymentMethodId);
        if (paymentMethodResult.IsFailure)
            return Result<bool>.Failure(paymentMethodResult.Error);

        var paymentMethod = paymentMethodResult.Value;
        if (paymentMethod.TenantId.Value != tenantId)
            return Result<bool>.Failure("Payment method does not belong to this tenant.");

        var deleteResult = await _paymentMethodRepository.Delete(paymentMethodId);
        if (deleteResult.IsFailure)
            return Result<bool>.Failure(deleteResult.Error);

        return Result<bool>.Success(true);
    }
}
