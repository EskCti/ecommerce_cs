using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.Services;

public static class SaleFinalizationPolicy
{
    /// <summary>
    /// RN-044: cart not empty; RN-043: change &gt;= 0; RN-041: credit requires customer.
    /// </summary>
    public static Result Validate(
        CashSession session,
        PaymentTerms paymentTerms,
        Guid? customerId,
        decimal amountPaid,
        decimal totalAfterDiscount)
    {
        if (session.IsCartEmpty())
            return Result.Failure("Cannot finalize an empty cart.");

        if (!session.HasReadyLines())
            return Result.Failure("Cart has lines pending grade confirmation.");

        if (session.Lines.Any(l => l.Status != SaleLineStatus.Ready))
            return Result.Failure("All cart lines must be ready before finalization.");

        if (paymentTerms == PaymentTerms.Credit && customerId is null)
            return Result.Failure("Customer is required for credit sales.");

        var change = amountPaid - totalAfterDiscount;
        if (change < 0)
            return Result.Failure("Change amount cannot be negative.");

        var changeResult = ChangeAmount.Create(change);
        return changeResult.IsFailure
            ? Result.Failure(changeResult.Error)
            : Result.Success();
    }
}
