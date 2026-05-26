using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Domain.ValueObjects;

public record PersonName
{
    public string Value { get; }

    private PersonName(string value) => Value = value;

    public static Result<PersonName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<PersonName>.Failure("Person name is required.");

        var trimmed = value.Trim();

        if (trimmed.Length < 2)
            return Result<PersonName>.Failure("Person name must be at least 2 characters long.");

        if (trimmed.Length > 100)
            return Result<PersonName>.Failure("Person name cannot exceed 100 characters.");

        return Result<PersonName>.Success(new PersonName(trimmed));
    }

    public static implicit operator string(PersonName name) => name.Value;
}
