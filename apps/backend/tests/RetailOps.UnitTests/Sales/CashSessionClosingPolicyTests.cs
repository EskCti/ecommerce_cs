using RetailOps.Core.Sales.Domain.Services;
using Xunit;

namespace RetailOps.UnitTests.Sales;

public class CashSessionClosingPolicyTests
{
    [Fact]
    public void CalculateBreakage_AppliesRn040Formula()
    {
        var result = CashSessionClosingPolicy.CalculateBreakage(
            countedCash: 150m,
            openingFloat: 100m,
            totalSold: 80m,
            totalWithdrawals: 20m);

        Assert.True(result.IsSuccess);
        Assert.Equal(-10m, result.Value!.Value);
    }

    [Fact]
    public void CalculateBreakage_WhenCountedMatchesExpected_ReturnsZero()
    {
        var result = CashSessionClosingPolicy.CalculateBreakage(160m, 100m, 80m, 20m);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value!.Value);
    }
}
