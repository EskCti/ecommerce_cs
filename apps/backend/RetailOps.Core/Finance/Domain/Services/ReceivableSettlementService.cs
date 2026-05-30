using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Finance.Domain.Services;

public sealed class ReceivableSettlementService
{
    public Result ValidateSettlement(Receivable receivable, DateTime settlementDateUtc)
    {
        if (receivable.Status == PaymentStatus.Settled)
            return Result.Failure("Receivable is already settled.");

        if (settlementDateUtc == default)
            return Result.Failure("Settlement date is required.");

        if (settlementDateUtc.Date < receivable.CreatedAt.Date)
            return Result.Failure("Settlement date cannot be before receivable creation.");

        return Result.Success();
    }

    public Result Settle(Receivable receivable, DateTime settlementDateUtc)
    {
        var validation = ValidateSettlement(receivable, settlementDateUtc);
        if (validation.IsFailure)
            return validation;

        return receivable.Settle(settlementDateUtc);
    }
}
