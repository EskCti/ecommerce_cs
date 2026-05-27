using RetailOps.Core.Catalog.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Catalog;

public class BarcodeTests
{
    [Fact]
    public void Create_WithValidValue_ReturnsSuccess()
    {
        var result = Barcode.Create("7891234567890");

        Assert.True(result.IsSuccess);
        Assert.Equal("7891234567890", result.Value!.Value);
    }

    [Fact]
    public void Create_TrimsWhitespace_ReturnsSuccess()
    {
        var result = Barcode.Create("  7891234567890  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("7891234567890", result.Value!.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyValue_ReturnsFailure(string? value)
    {
        var result = Barcode.Create(value!);

        Assert.True(result.IsFailure);
        Assert.Contains("required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_WhenExceedsMaxLength_ReturnsFailure()
    {
        var result = Barcode.Create(new string('1', 51));

        Assert.True(result.IsFailure);
        Assert.Contains("50", result.Error);
    }
}
