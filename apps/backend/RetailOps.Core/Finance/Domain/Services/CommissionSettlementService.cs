using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Domain.Services;

public sealed class CommissionSettlementService
{
    public Result<Payable> CreatePaymentPayable(Commission commission, DateTime paidAtUtc)
    {
        if (commission.IsPaid)
            return Result<Payable>.Failure("Commission is already paid.");

        var dueDateResult = DueDate.Create(paidAtUtc);
        if (dueDateResult.IsFailure)
            return Result<Payable>.Failure(dueDateResult.Error);

        return Payable.CreateCommissionPayment(
            commission.TenantId,
            commission.Amount,
            dueDateResult.Value,
            commission.Id,
            commission.SellerLegacyId);
    }

    public Result Pay(Commission commission, Payable paymentPayable, DateTime paidAtUtc)
    {
        if (paymentPayable.Type != AccountType.CommissionPayment)
            return Result.Failure("Invalid commission payment payable type.");

        if (paymentPayable.CommissionId != commission.Id)
            return Result.Failure("Payment payable does not match commission.");

        var markPaid = commission.MarkPaid(paymentPayable.Id, paidAtUtc);
        return markPaid;
    }
}
