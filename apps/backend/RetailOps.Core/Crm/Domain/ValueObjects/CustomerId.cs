using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Domain.ValueObjects;

public record CustomerId
{
    public Guid Value { get; }

    private CustomerId(Guid value) => Value = value;

    public static Result<CustomerId> Create(Guid value)
    {
        if (value == Guid.Empty)
            return Result<CustomerId>.Failure("Customer id is required.");

        return Result<CustomerId>.Success(new CustomerId(value));
    }

    public static implicit operator Guid(CustomerId id) => id.Value;
}
