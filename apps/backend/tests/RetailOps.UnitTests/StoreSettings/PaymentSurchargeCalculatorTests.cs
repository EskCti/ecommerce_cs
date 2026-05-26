using RetailOps.Core.StoreSettings.Domain.Services;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class PaymentSurchargeCalculatorTests
{
    private readonly PaymentSurchargeCalculator _calculator = new();

    [Fact]
    public void Calculate_WithZeroSurcharge_ReturnsBaseAmount()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod(surcharge: 0);
        var baseAmount = new Money(100m, "BRL");

        var total = _calculator.Calculate(baseAmount, method);

        Assert.Equal(100m, total.Amount);
    }

    [Fact]
    public void Calculate_WithTenPercentSurcharge_AddsTenPercent()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod(surcharge: 10);
        var baseAmount = new Money(200m, "BRL");

        var total = _calculator.Calculate(baseAmount, method);

        Assert.Equal(220m, total.Amount);
    }

    [Fact]
    public void Calculate_WithInactiveMethod_ReturnsBaseAmount()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod(surcharge: 15, isActive: false);
        var baseAmount = new Money(100m, "BRL");

        var total = _calculator.Calculate(baseAmount, method);

        Assert.Equal(100m, total.Amount);
    }

    [Fact]
    public void CalculateWithMaxLimit_CapsSurcharge()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod(surcharge: 50);
        var baseAmount = new Money(100m, "BRL");
        var maxSurcharge = new Money(10m, "BRL");

        var total = _calculator.CalculateWithMaxLimit(baseAmount, method, maxSurcharge);

        Assert.Equal(110m, total.Amount);
    }
}
