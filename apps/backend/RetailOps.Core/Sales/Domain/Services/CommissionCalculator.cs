using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.Services;

public static class CommissionCalculator
{
    /// <summary>
    /// RN-046: commission percent applied on sale total.
    /// </summary>
    public static Result<decimal> Calculate(decimal saleTotal, decimal commissionPercent)
    {
        if (saleTotal < 0)
            return Result<decimal>.Failure("Sale total cannot be negative.");

        if (commissionPercent < 0)
            return Result<decimal>.Failure("Commission percent cannot be negative.");

        var amount = saleTotal * commissionPercent / 100m;
        return Result<decimal>.Success(Math.Round(amount, 2));
    }
}
