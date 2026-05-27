using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.ValueObjects;

public record SalePrice
{
    public decimal Value { get; }
    public bool IsOpenPrice { get; }

    private SalePrice(decimal value, bool isOpenPrice)
    {
        Value = value;
        IsOpenPrice = isOpenPrice;
    }

    public static Result<SalePrice> Create(decimal value)
    {
        if (value < 0)
            return Result<SalePrice>.Failure("Sale price cannot be negative.");

        var isOpenPrice = value == 0;
        return Result<SalePrice>.Success(new SalePrice(value, isOpenPrice));
    }
}
