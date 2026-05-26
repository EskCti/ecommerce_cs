using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class StoreSettingsValueObjectsTests
{
    [Theory]
    [InlineData("PDF")]
    [InlineData("HTML")]
    [InlineData("EXCEL")]
    [InlineData("CSV")]
    public void ReportFormat_Create_WithValidValue_Succeeds(string format)
    {
        var result = ReportFormat.Create(format);
        Assert.True(result.IsSuccess);
        Assert.Equal(format, result.Value!.Value);
    }

    [Fact]
    public void ReportFormat_FromLegacyValue_MapsCorrectly()
    {
        var format = ReportFormat.FromLegacyValue("H");
        Assert.Equal("HTML", format.Value);
    }

    [Fact]
    public void ReportFormat_ToLegacyValue_MapsPdf()
    {
        var format = ReportFormat.Create("PDF").Value!;
        Assert.Equal("P", format.ToLegacyValue());
    }

    [Fact]
    public void ReportFormat_Create_WithInvalidValue_ReturnsFailure()
    {
        Assert.True(ReportFormat.Create("DOC").IsFailure);
        Assert.True(ReportFormat.Create("").IsFailure);
    }

    [Fact]
    public void ReportFormat_FromLegacyValue_DefaultsToPdf()
    {
        Assert.Equal("PDF", ReportFormat.FromLegacyValue("?").Value);
    }

    [Fact]
    public void ReportFormat_ToLegacyValue_MapsAllFormats()
    {
        Assert.Equal("E", ReportFormat.Create("EXCEL").Value!.ToLegacyValue());
        Assert.Equal("C", ReportFormat.Create("CSV").Value!.ToLegacyValue());
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void CommissionRate_Create_ValidatesRange(decimal value, bool expectedSuccess)
    {
        var result = CommissionRate.Create(value);
        Assert.Equal(expectedSuccess, result.IsSuccess);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void SurchargePercent_Create_ValidatesRange(decimal value, bool expectedSuccess)
    {
        var result = SurchargePercent.Create(value);
        Assert.Equal(expectedSuccess, result.IsSuccess);
    }

    [Fact]
    public void PaymentMethodName_Create_RejectsSingleCharacter()
    {
        var result = PaymentMethodName.Create("A");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CashRegisterTerminalName_Create_RejectsEmpty()
    {
        var result = CashRegisterTerminalName.Create("");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ImagePath_Create_AcceptsRelativePath()
    {
        var result = ImagePath.Create("/images/logo.png");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ApiToken_Create_RequiresMinimumLength()
    {
        Assert.True(ApiToken.Create("1234567890").IsSuccess);
        Assert.True(ApiToken.Create("short").IsFailure);
    }

    [Fact]
    public void UserId_Create_RejectsEmptyGuid()
    {
        Assert.True(UserId.Create(Guid.Empty).IsFailure);
    }

    [Fact]
    public void UserId_Create_FromInvalidString_ReturnsFailure()
    {
        Assert.True(UserId.Create("not-a-guid").IsFailure);
        Assert.True(UserId.Create("").IsFailure);
    }

    [Fact]
    public void UserId_Empty_IsEmpty()
    {
        Assert.True(UserId.Empty.IsEmpty);
    }

    [Fact]
    public void ImagePath_Create_RejectsInvalidExtension()
    {
        Assert.True(ImagePath.Create("/images/logo.pdf").IsFailure);
    }

    [Fact]
    public void StoreName_Create_RejectsTooShortName()
    {
        Assert.True(StoreName.Create("AB").IsFailure);
    }

    [Fact]
    public void PaymentMethodName_Create_RejectsTooLongName()
    {
        Assert.True(PaymentMethodName.Create(new string('A', 51)).IsFailure);
    }

    [Fact]
    public void ReportFormat_FromLegacyValue_MapsAllLegacyCodes()
    {
        Assert.Equal("PDF", ReportFormat.FromLegacyValue("P").Value);
        Assert.Equal("EXCEL", ReportFormat.FromLegacyValue("E").Value);
        Assert.Equal("CSV", ReportFormat.FromLegacyValue("C").Value);
    }

    [Fact]
    public void ApiToken_Create_RejectsTooLongValue()
    {
        Assert.True(ApiToken.Create(new string('a', 256)).IsFailure);
    }

    [Fact]
    public void CashRegisterTerminalName_Create_RejectsTooLongName()
    {
        Assert.True(CashRegisterTerminalName.Create(new string('A', 51)).IsFailure);
    }

    [Fact]
    public void Cnpj_Create_RejectsRepeatedDigits()
    {
        Assert.True(Cnpj.Create("11.111.111/1111-11").IsFailure);
    }
}
