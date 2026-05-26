using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class CnpjTests
{
    private const string ValidFormatted = "12.345.678/0001-95";
    private const string ValidDigits = "12345678000195";
    private const string OtherValidFormatted = "11.444.777/0001-61";

    [Fact]
    public void Create_WithValidFormattedCnpj_ReturnsSuccessWithDigitsOnly()
    {
        var result = Cnpj.Create(ValidFormatted);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidDigits, result.Value!.Value);
    }

    [Fact]
    public void Create_WithValidDigitsOnly_ReturnsSuccess()
    {
        var result = Cnpj.Create(ValidDigits);

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidDigits, result.Value!.Value);
    }

    [Fact]
    public void Create_WithEmptyString_ReturnsFailure()
    {
        var result = Cnpj.Create("");

        Assert.True(result.IsFailure);
        Assert.Equal("CNPJ is required.", result.Error);
    }

    [Fact]
    public void Create_WithWhitespaceOnly_ReturnsFailure()
    {
        var result = Cnpj.Create("   ");

        Assert.True(result.IsFailure);
        Assert.Equal("CNPJ is required.", result.Error);
    }

    [Fact]
    public void Create_WithInvalidChecksum_ReturnsFailure()
    {
        var result = Cnpj.Create("12345678901234");

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid CNPJ.", result.Error);
    }

    [Fact]
    public void Create_WithInvalidCharacters_ReturnsFailure()
    {
        var result = Cnpj.Create("12.345.678/0001-XX");

        Assert.True(result.IsFailure);
        Assert.Equal("CNPJ must have 14 digits.", result.Error);
    }

    [Fact]
    public void Create_WithWrongLength_ReturnsFailure()
    {
        var result = Cnpj.Create("12.345.678/0001-9");

        Assert.True(result.IsFailure);
        Assert.Equal("CNPJ must have 14 digits.", result.Error);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var cnpj1 = Cnpj.Create(ValidFormatted).Value;
        var cnpj2 = Cnpj.Create(ValidDigits).Value;

        Assert.True(cnpj1!.Equals(cnpj2));
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var cnpj1 = Cnpj.Create(ValidFormatted).Value;
        var cnpj2 = Cnpj.Create(OtherValidFormatted).Value;

        Assert.False(cnpj1!.Equals(cnpj2));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var cnpj1 = Cnpj.Create(ValidFormatted).Value;

        Assert.False(cnpj1!.Equals(null));
    }
}
