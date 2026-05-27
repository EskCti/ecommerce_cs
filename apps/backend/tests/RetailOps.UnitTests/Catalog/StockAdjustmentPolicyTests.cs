using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Catalog;

public class StockAdjustmentPolicyTests
{
    private readonly StockAdjustmentPolicy _sut = new();

    [Fact]
    public void ValidateExit_WhenExitExceedsStock_ReturnsFailure()
    {
        var current = StockQuantity.Create(5).Value;
        var exit = StockQuantity.Create(6).Value;

        var result = _sut.ValidateExit(current, exit);

        Assert.True(result.IsFailure);
        Assert.Contains("cannot exceed", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateExit_WhenExitWithinStock_ReturnsSuccess()
    {
        var current = StockQuantity.Create(10).Value;
        var exit = StockQuantity.Create(4).Value;

        var result = _sut.ValidateExit(current, exit);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateAdjustment_WhenReducingWithinStock_ReturnsSuccess()
    {
        var current = StockQuantity.Create(8).Value;
        var newStock = StockQuantity.Create(2).Value;

        var result = _sut.ValidateAdjustment(current, newStock, isExit: true);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateAdjustment_WhenNewStockIsNegative_ReturnsFailure()
    {
        var createResult = StockQuantity.Create(-1);

        Assert.True(createResult.IsFailure);
        Assert.Contains("negative", createResult.Error, StringComparison.OrdinalIgnoreCase);
    }
}
