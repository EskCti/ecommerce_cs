using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Domain.ValueObjects;

public record Phone
{
    public string Value { get; }

    private Phone(string value) => Value = value;

    public static Result<Phone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Phone>.Failure("Phone is required.");

        var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '+').ToArray());

        if (cleaned.Length < 8)
            return Result<Phone>.Failure("Phone must have at least 8 digits.");

        if (cleaned.Length > 20)
            return Result<Phone>.Failure("Phone cannot exceed 20 characters.");

        return Result<Phone>.Success(new Phone(cleaned));
    }

    public static implicit operator string(Phone phone) => phone.Value;
}
