using RetailOps.Core.Sales.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Sales;

public class ScanQuantityPrefixTests
{
    [Fact]
    public void Parse_WithTwoStarPrefix_ReturnsQuantityTwo()
    {
        var result = ScanQuantityPrefix.Parse("2*789123");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Quantity);
        Assert.Equal("789123", result.Value.Barcode);
    }

    [Fact]
    public void Parse_WithoutPrefix_ReturnsQuantityOne()
    {
        var result = ScanQuantityPrefix.Parse("789123");

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Quantity);
        Assert.Equal("789123", result.Value.Barcode);
    }
}
