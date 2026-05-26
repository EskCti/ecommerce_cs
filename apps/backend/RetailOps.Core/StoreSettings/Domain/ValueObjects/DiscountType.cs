namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public enum DiscountType
{
    None = 0,
    Percentage = 1,
    FixedAmount = 2
}

public static class DiscountTypeExtensions
{
    public static string ToLegacyValue(this DiscountType discountType)
    {
        return discountType switch
        {
            DiscountType.None => "N",
            DiscountType.Percentage => "P",
            DiscountType.FixedAmount => "F",
            _ => "N"
        };
    }

    public static DiscountType FromLegacyValue(string legacyValue)
    {
        return legacyValue?.ToUpperInvariant() switch
        {
            "P" => DiscountType.Percentage,
            "F" => DiscountType.FixedAmount,
            _ => DiscountType.None
        };
    }
}