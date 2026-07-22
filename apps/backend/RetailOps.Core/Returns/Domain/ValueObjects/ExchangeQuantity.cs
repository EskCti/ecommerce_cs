using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Returns.Domain.ValueObjects;

public record ExchangeQuantity
{
    public const int FixedValue = 1;

    public int Value { get; }

    private ExchangeQuantity(int value) => Value = value;

    public static Result<ExchangeQuantity> Create(int value)
    {
        if (value != FixedValue)
            return Result<ExchangeQuantity>.Failure("Exchange quantity must be exactly 1 per RN-050.");

        return Result<ExchangeQuantity>.Success(new ExchangeQuantity(value));
    }

    public static Result<ExchangeQuantity> One() => Create(FixedValue);
}
