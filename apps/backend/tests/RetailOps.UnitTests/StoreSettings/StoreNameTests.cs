using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class StoreNameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsSuccess()
    {
        var result = StoreName.Create("Minha Loja");

        Assert.True(result.IsSuccess);
        Assert.Equal("Minha Loja", result.Value!.Value);
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        var result = StoreName.Create("");

        Assert.True(result.IsFailure);
        Assert.Equal("Store name is required.", result.Error);
    }

    [Fact]
    public void Create_WithWhitespaceOnly_ReturnsFailure()
    {
        var result = StoreName.Create("   ");

        Assert.True(result.IsFailure);
        Assert.Equal("Store name is required.", result.Error);
    }

    [Fact]
    public void Create_WithNameTooLong_ReturnsFailure()
    {
        var longName = new string('A', 101);
        var result = StoreName.Create(longName);

        Assert.True(result.IsFailure);
        Assert.Equal("Store name cannot exceed 100 characters.", result.Error);
    }

    [Fact]
    public void Create_WithNameExactly100Characters_ReturnsSuccess()
    {
        var exactName = new string('A', 100);
        var result = StoreName.Create(exactName);

        Assert.True(result.IsSuccess);
        Assert.Equal(exactName, result.Value!.Value);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var name1 = StoreName.Create("Loja A").Value;
        var name2 = StoreName.Create("Loja A").Value;

        Assert.True(name1!.Equals(name2));
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var name1 = StoreName.Create("Loja A").Value;
        var name2 = StoreName.Create("Loja B").Value;

        Assert.False(name1!.Equals(name2));
    }
}
