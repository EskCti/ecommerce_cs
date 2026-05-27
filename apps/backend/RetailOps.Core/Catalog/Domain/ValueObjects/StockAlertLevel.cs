using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.ValueObjects;

public record StockAlertLevel
{
    public int Value { get; }

    private StockAlertLevel(int value) => Value = value;

    public static Result<StockAlertLevel> Create(int value)
    {
        if (value < 0)
            return Result<StockAlertLevel>.Failure("Stock alert level cannot be negative.");

        return Result<StockAlertLevel>.Success(new StockAlertLevel(value));
    }
}
