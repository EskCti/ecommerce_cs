using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.ValueObjects;

public record Discount
{
    public decimal Value { get; }

    private Discount(decimal value) => Value = value;

    public static Result<Discount> Create(decimal value)
    {
        if (value < 0)
            return Result<Discount>.Failure("Discount cannot be negative.");

        return Result<Discount>.Success(new Discount(Math.Round(value, 2)));
    }
}
