using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.ValueObjects;

public record StockQuantity
{
    public int Value { get; }

    private StockQuantity(int value) => Value = value;

    public static Result<StockQuantity> Create(int value)
    {
        if (value < 0)
            return Result<StockQuantity>.Failure("Stock quantity cannot be negative.");

        return Result<StockQuantity>.Success(new StockQuantity(value));
    }

    public static Result<StockQuantity> Zero() => Success(0);

    private static Result<StockQuantity> Success(int value) =>
        Result<StockQuantity>.Success(new StockQuantity(value));
}
