using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.ValueObjects;

public record UnitPrice
{
    public decimal Value { get; }

    private UnitPrice(decimal value) => Value = value;

    public static Result<UnitPrice> Create(decimal value)
    {
        if (value < 0)
            return Result<UnitPrice>.Failure("Unit price cannot be negative.");

        return Result<UnitPrice>.Success(new UnitPrice(Math.Round(value, 2)));
    }
}
