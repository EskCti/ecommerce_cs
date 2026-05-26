using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class PaymentMethodTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = PaymentMethod.Create(
            StoreSettingsTestHelpers.Tenant(),
            PaymentMethodName.Create("Cartão").Value,
            SurchargePercent.Create(3).Value);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsActive);
    }

    [Fact]
    public void Rename_UpdatesName()
    {
        var paymentMethod = StoreSettingsTestHelpers.CreatePaymentMethod();
        var result = paymentMethod.Rename(PaymentMethodName.Create("Dinheiro").Value);

        Assert.True(result.IsSuccess);
        Assert.Equal("Dinheiro", paymentMethod.Name.Value);
    }

    [Fact]
    public void UpdateSurcharge_UpdatesPercent()
    {
        var paymentMethod = StoreSettingsTestHelpers.CreatePaymentMethod();
        var result = paymentMethod.UpdateSurcharge(SurchargePercent.Create(7).Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, paymentMethod.SurchargePercent.Value);
    }

    [Fact]
    public void Deactivate_SetsInactive()
    {
        var paymentMethod = StoreSettingsTestHelpers.CreatePaymentMethod();
        var result = paymentMethod.Deactivate();

        Assert.True(result.IsSuccess);
        Assert.False(paymentMethod.IsActive);
    }

    [Fact]
    public void Activate_SetsActive()
    {
        var paymentMethod = StoreSettingsTestHelpers.CreatePaymentMethod(isActive: false);
        var result = paymentMethod.Activate();

        Assert.True(result.IsSuccess);
        Assert.True(paymentMethod.IsActive);
    }
}
