using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.ValueObjects;

public record WarrantyDays
{
    public int Value { get; }

    private WarrantyDays(int value) => Value = value;

    public static Result<WarrantyDays> Create(int value)
    {
        if (value < 0)
            return Result<WarrantyDays>.Failure("Warranty days cannot be negative.");

        return Result<WarrantyDays>.Success(new WarrantyDays(value));
    }
}
