using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Catalog;

public class ProfitMarginCalculatorTests
{
    [Fact]
    public void Calculate_WhenCostIsZero_ReturnsZeroMargin()
    {
        var sale = SalePrice.Create(100m).Value;
        var cost = CostPrice.Create(0m).Value;

        var result = ProfitMarginCalculator.Calculate(sale, cost);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value!.Value);
    }

    [Fact]
    public void Calculate_WithPositiveMargin_AppliesRn022Formula()
    {
        var sale = SalePrice.Create(110m).Value;
        var cost = CostPrice.Create(100m).Value;

        var result = ProfitMarginCalculator.Calculate(sale, cost);

        Assert.True(result.IsSuccess);
        Assert.Equal(10m, result.Value!.Value);
    }

    [Fact]
    public void Calculate_WithNegativeMargin_RoundsToTwoDecimals()
    {
        var sale = SalePrice.Create(75.5m).Value;
        var cost = CostPrice.Create(100m).Value;

        var result = ProfitMarginCalculator.Calculate(sale, cost);

        Assert.True(result.IsSuccess);
        Assert.Equal(-24.5m, result.Value!.Value);
    }
}
