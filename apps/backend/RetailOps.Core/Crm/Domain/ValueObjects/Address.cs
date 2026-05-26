using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Domain.ValueObjects;

public record Address
{
    public string Value { get; }

    private Address(string value) => Value = value;

    public static Result<Address> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Address>.Failure("Address is required.");

        var trimmed = value.Trim();

        if (trimmed.Length < 5)
            return Result<Address>.Failure("Address must be at least 5 characters long.");

        if (trimmed.Length > 200)
            return Result<Address>.Failure("Address cannot exceed 200 characters.");

        return Result<Address>.Success(new Address(trimmed));
    }

    public static implicit operator string(Address address) => address.Value;
}
