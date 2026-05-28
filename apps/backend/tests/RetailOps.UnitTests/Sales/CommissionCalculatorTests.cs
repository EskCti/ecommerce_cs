using RetailOps.Core.Sales.Domain.Services;
using Xunit;

namespace RetailOps.UnitTests.Sales;

public class CommissionCalculatorTests
{
    [Fact]
    public void Calculate_AppliesRn046PercentOnTotal()
    {
        var result = CommissionCalculator.Calculate(200m, 5m);

        Assert.True(result.IsSuccess);
        Assert.Equal(10m, result.Value);
    }

    [Fact]
    public void Calculate_WithZeroPercent_ReturnsZero()
    {
        var result = CommissionCalculator.Calculate(150m, 0m);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value);
    }
}
