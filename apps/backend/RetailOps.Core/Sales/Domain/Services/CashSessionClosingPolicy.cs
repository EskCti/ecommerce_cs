using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.Services;

public static class CashSessionClosingPolicy
{
    /// <summary>
    /// RN-040: breakage = counted - (opening + sold - withdrawals)
    /// </summary>
    public static Result<CashBreakage> CalculateBreakage(
        decimal countedCash,
        decimal openingFloat,
        decimal totalSold,
        decimal totalWithdrawals)
    {
        var expected = openingFloat + totalSold - totalWithdrawals;
        var breakage = countedCash - expected;
        return CashBreakage.Create(breakage);
    }
}
