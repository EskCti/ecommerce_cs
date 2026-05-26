using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class StoreConfigTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = StoreConfig.Create(
            StoreSettingsTestHelpers.Tenant(),
            StoreName.Create("Minha Loja").Value,
            cnpj: null,
            DiscountType.None,
            discountValue: 0,
            CommissionRate.Create(0).Value,
            ReportFormat.Create("PDF").Value);

        Assert.True(result.IsSuccess);
        Assert.Equal("Minha Loja", result.Value.Name.Value);
    }

    [Fact]
    public void Create_WithPercentageDiscountAndZeroValue_Succeeds()
    {
        var result = StoreConfig.Create(
            StoreSettingsTestHelpers.Tenant(),
            StoreName.Create("Loja").Value,
            null,
            DiscountType.Percentage,
            15,
            CommissionRate.Create(3).Value,
            ReportFormat.Create("HTML").Value);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithInvalidDiscountCombination_ReturnsFailure()
    {
        var result = StoreConfig.Create(
            StoreSettingsTestHelpers.Tenant(),
            StoreName.Create("Loja").Value,
            null,
            DiscountType.None,
            10,
            CommissionRate.Create(0).Value,
            ReportFormat.Create("PDF").Value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void UpdateGeneralInfo_UpdatesFields()
    {
        var config = StoreSettingsTestHelpers.CreateStoreConfig();
        var result = config.UpdateGeneralInfo(
            StoreName.Create("Nova Loja").Value,
            Cnpj.Create("12.345.678/0001-95").Value,
            ImagePath.Create("/logo.png").Value,
            "11999999999",
            "Av. Brasil, 100");

        Assert.True(result.IsSuccess);
        Assert.Equal("Nova Loja", config.Name.Value);
        Assert.Equal("11999999999", config.Contacts);
    }

    [Fact]
    public void UpdateDiscountSettings_WithFixedAmountNegative_ReturnsFailure()
    {
        var config = StoreSettingsTestHelpers.CreateStoreConfig();
        var result = config.UpdateDiscountSettings(DiscountType.FixedAmount, -1);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void UpdateIntegrationToken_ClearsToken()
    {
        var config = StoreSettingsTestHelpers.CreateStoreConfig();
        config.UpdateIntegrationToken(ApiToken.Create("abcdefghij").Value);
        var result = config.UpdateIntegrationToken(null);

        Assert.True(result.IsSuccess);
        Assert.Null(config.ApiToken);
    }

    [Fact]
    public void UpdateReportSettings_UpdatesCommissionAndFormat()
    {
        var config = StoreSettingsTestHelpers.CreateStoreConfig();
        var result = config.UpdateReportSettings(
            CommissionRate.Create(10).Value,
            ReportFormat.Create("CSV").Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, config.CommissionRate.Value);
        Assert.Equal("CSV", config.ReportFormat.Value);
    }

    [Fact]
    public void UpdateDiscountSettings_WithPercentageAbove100_ReturnsFailure()
    {
        var config = StoreSettingsTestHelpers.CreateStoreConfig();
        var result = config.UpdateDiscountSettings(DiscountType.Percentage, 101);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithOptionalFields_Succeeds()
    {
        var result = StoreConfig.Create(
            StoreSettingsTestHelpers.Tenant(),
            StoreName.Create("Loja").Value,
            Cnpj.Create("12.345.678/0001-95").Value,
            DiscountType.FixedAmount,
            20,
            CommissionRate.Create(1).Value,
            ReportFormat.Create("PDF").Value,
            ApiToken.Create("1234567890").Value,
            ImagePath.Create("/images/logo.png").Value,
            "contato",
            "endereco");

        Assert.True(result.IsSuccess);
        Assert.Equal("contato", result.Value.Contacts);
    }

    [Fact]
    public void Reconstitute_WithValidData_ReturnsSuccess()
    {
        var result = StoreConfig.Reconstitute(
            StoreSettingsTestHelpers.StoreConfigId(),
            StoreSettingsTestHelpers.Tenant(),
            StoreName.Create("Loja").Value,
            null,
            DiscountType.None,
            0,
            CommissionRate.Create(0).Value,
            ReportFormat.Create("PDF").Value,
            null,
            null,
            "contato",
            "endereco",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal("contato", result.Value.Contacts);
    }

    [Fact]
    public void Reconstitute_WithInvalidDiscount_ReturnsFailure()
    {
        var result = StoreConfig.Reconstitute(
            StoreSettingsTestHelpers.StoreConfigId(),
            StoreSettingsTestHelpers.Tenant(),
            StoreName.Create("Loja").Value,
            null,
            DiscountType.None,
            10,
            CommissionRate.Create(0).Value,
            ReportFormat.Create("PDF").Value,
            null,
            null,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void UpdateGeneralInfo_WithNullCnpj_ClearsCnpj()
    {
        var config = StoreSettingsTestHelpers.CreateStoreConfig();
        config.UpdateGeneralInfo(
            StoreName.Create("Loja").Value,
            Cnpj.Create("12.345.678/0001-95").Value,
            null,
            null,
            null);

        var result = config.UpdateGeneralInfo(
            StoreName.Create("Loja").Value,
            null,
            null,
            null,
            null);

        Assert.True(result.IsSuccess);
        Assert.Null(config.Cnpj);
    }
}
