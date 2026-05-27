using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Catalog;

public class LowStockPolicyTests
{
    [Fact]
    public void IsLowStock_WhenStockBelowAlertLevel_ReturnsTrue()
    {
        var stock = StockQuantity.Create(3).Value;
        var alert = StockAlertLevel.Create(5).Value;

        Assert.True(LowStockPolicy.IsLowStock(stock, alert));
        Assert.True(LowStockPolicy.ShouldEmitLowStockDetected(stock, alert));
    }

    [Fact]
    public void IsLowStock_WhenStockEqualsAlertLevel_ReturnsFalse()
    {
        var stock = StockQuantity.Create(5).Value;
        var alert = StockAlertLevel.Create(5).Value;

        Assert.False(LowStockPolicy.IsLowStock(stock, alert));
    }

    [Fact]
    public void IsLowStock_WhenStockAboveAlertLevel_ReturnsFalse()
    {
        var stock = StockQuantity.Create(12).Value;
        var alert = StockAlertLevel.Create(5).Value;

        Assert.False(LowStockPolicy.IsLowStock(stock, alert));
    }
}
