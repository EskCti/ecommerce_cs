using RetailOps.Core.Finance.Application.DTOs;
using RetailOps.Core.Finance.Domain.Repositories;
using RetailOps.Core.Finance.Domain.Services;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.Transactions;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Application.UseCases;

public sealed class PayCommissionUseCase(
    ICommissionRepository commissionRepository,
    IPayableRepository payableRepository,
    CommissionSettlementService settlementService,
    ITransactionManager transactionManager)
    : IUseCase<(int tenantId, Guid commissionId, SettleAccountInputDto input), CommissionOutputDto>
{
    public async Task<Result<CommissionOutputDto>> Execute(
        (int tenantId, Guid commissionId, SettleAccountInputDto input) request)
    {
        var (tenantId, commissionId, input) = request;

        return await transactionManager.RunInTransactionAsync(async () =>
        {
            var commissionResult = await commissionRepository.GetById(commissionId);
            if (commissionResult.IsFailure)
                return Result<CommissionOutputDto>.Failure(commissionResult.Error);

            if (commissionResult.Value.TenantId.Value != tenantId)
                return Result<CommissionOutputDto>.Failure("Commission does not belong to this tenant.");

            var payableResult = settlementService.CreatePaymentPayable(
                commissionResult.Value,
                input.SettlementDate);

            if (payableResult.IsFailure)
                return Result<CommissionOutputDto>.Failure(payableResult.Error);

            var savePayable = await payableRepository.Save(payableResult.Value);
            if (savePayable.IsFailure)
                return Result<CommissionOutputDto>.Failure(savePayable.Error);

            var payResult = settlementService.Pay(
                commissionResult.Value,
                payableResult.Value,
                input.SettlementDate);

            if (payResult.IsFailure)
                return Result<CommissionOutputDto>.Failure(payResult.Error);

            var saveCommission = await commissionRepository.Save(commissionResult.Value);
            if (saveCommission.IsFailure)
                return Result<CommissionOutputDto>.Failure(saveCommission.Error);

            return Result<CommissionOutputDto>.Success(
                CommissionOutputDto.FromDomain(commissionResult.Value));
        });
    }
}

public sealed class PayCommissionsBatchUseCase(
    ICommissionRepository commissionRepository,
    IPayableRepository payableRepository,
    CommissionSettlementService settlementService,
    ITransactionManager transactionManager)
    : IUseCase<(int tenantId, PayCommissionsBatchInputDto input), IReadOnlyList<CommissionOutputDto>>
{
    public async Task<Result<IReadOnlyList<CommissionOutputDto>>> Execute(
        (int tenantId, PayCommissionsBatchInputDto input) request)
    {
        var (tenantId, input) = request;

        if (input.CommissionIds.Count == 0)
            return Result<IReadOnlyList<CommissionOutputDto>>.Failure("Select at least one commission.");

        return await transactionManager.RunInTransactionAsync(async () =>
        {
            var commissionsResult = await commissionRepository.GetByIds(input.CommissionIds.ToList());
            if (commissionsResult.IsFailure)
                return Result<IReadOnlyList<CommissionOutputDto>>.Failure(commissionsResult.Error);

            var outputs = new List<CommissionOutputDto>();

            foreach (var commission in commissionsResult.Value)
            {
                if (commission.TenantId.Value != tenantId)
                    return Result<IReadOnlyList<CommissionOutputDto>>.Failure("Commission tenant mismatch.");

                var payableResult = settlementService.CreatePaymentPayable(commission, input.PaymentDate);
                if (payableResult.IsFailure)
                    return Result<IReadOnlyList<CommissionOutputDto>>.Failure(payableResult.Error);

                var savePayable = await payableRepository.Save(payableResult.Value);
                if (savePayable.IsFailure)
                    return Result<IReadOnlyList<CommissionOutputDto>>.Failure(savePayable.Error);

                var payResult = settlementService.Pay(commission, payableResult.Value, input.PaymentDate);
                if (payResult.IsFailure)
                    return Result<IReadOnlyList<CommissionOutputDto>>.Failure(payResult.Error);

                var saveCommission = await commissionRepository.Save(commission);
                if (saveCommission.IsFailure)
                    return Result<IReadOnlyList<CommissionOutputDto>>.Failure(saveCommission.Error);

                outputs.Add(CommissionOutputDto.FromDomain(commission));
            }

            return Result<IReadOnlyList<CommissionOutputDto>>.Success(outputs);
        });
    }
}
