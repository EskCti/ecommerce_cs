using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Finance.Domain.Services;

public sealed class PayableSettlementService
{
    public Result ValidateSettlement(Payable payable, DateTime settlementDateUtc)
    {
        if (payable.Status == PaymentStatus.Settled)
            return Result.Failure("Payable is already settled.");

        if (settlementDateUtc == default)
            return Result.Failure("Settlement date is required.");

        return Result.Success();
    }

    public Result Settle(Payable payable, DateTime settlementDateUtc)
    {
        var validation = ValidateSettlement(payable, settlementDateUtc);
        if (validation.IsFailure)
            return validation;

        return payable.Settle(settlementDateUtc);
    }

    public PaymentStatus ResolveInitialStatus(DueDate dueDate, DateTime referenceUtc) =>
        dueDate.IsFuture(referenceUtc) ? PaymentStatus.Open : PaymentStatus.Settled;
}
