using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.ValueObjects;

public record CostPrice
{
    public decimal Value { get; }

    private CostPrice(decimal value) => Value = value;

    public static Result<CostPrice> Create(decimal value)
    {
        if (value < 0)
            return Result<CostPrice>.Failure("Cost price cannot be negative.");

        return Result<CostPrice>.Success(new CostPrice(value));
    }
}
