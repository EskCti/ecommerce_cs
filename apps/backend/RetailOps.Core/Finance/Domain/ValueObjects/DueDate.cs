using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Finance.Domain.ValueObjects;

public record DueDate
{
    public DateTime Value { get; }

    private DueDate(DateTime value) => Value = value;

    public static Result<DueDate> Create(DateTime value)
    {
        if (value == default)
            return Result<DueDate>.Failure("Due date is required.");

        return Result<DueDate>.Success(new DueDate(DateTime.SpecifyKind(value.Date, DateTimeKind.Utc)));
    }

    public bool IsFuture(DateTime referenceUtc) => Value.Date > referenceUtc.Date;
}
