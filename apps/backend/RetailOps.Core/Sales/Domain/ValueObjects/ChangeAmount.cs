using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.ValueObjects;

public record ChangeAmount
{
    public decimal Value { get; }

    private ChangeAmount(decimal value) => Value = value;

    public static Result<ChangeAmount> Create(decimal value)
    {
        if (value < 0)
            return Result<ChangeAmount>.Failure("Change amount cannot be negative.");

        return Result<ChangeAmount>.Success(new ChangeAmount(Math.Round(value, 2)));
    }
}
