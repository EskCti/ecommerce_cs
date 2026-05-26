using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Entities;

namespace RetailOps.Core.StoreSettings.Domain.Services;

public sealed class PaymentSurchargeCalculator
{
    public Money Calculate(Money baseAmount, PaymentMethod paymentMethod)
    {
        if (!paymentMethod.IsActive)
            return baseAmount;

        var surchargePercent = paymentMethod.SurchargePercent.Value;
        
        if (surchargePercent == 0)
            return baseAmount;

        var surchargeAmount = baseAmount.Amount * (surchargePercent / 100);
        var totalAmount = baseAmount.Amount + surchargeAmount;

        return new Money(totalAmount, baseAmount.Currency);
    }

    public Money CalculateWithMaxLimit(Money baseAmount, PaymentMethod paymentMethod, Money maxSurcharge)
    {
        var totalWithSurcharge = Calculate(baseAmount, paymentMethod);
        var surchargeAmount = totalWithSurcharge.Amount - baseAmount.Amount;

        if (surchargeAmount <= maxSurcharge.Amount)
            return totalWithSurcharge;

        var limitedTotal = baseAmount.Amount + maxSurcharge.Amount;
        return new Money(limitedTotal, baseAmount.Currency);
    }
}