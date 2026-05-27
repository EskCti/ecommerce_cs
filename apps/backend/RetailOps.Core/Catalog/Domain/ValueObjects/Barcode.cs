using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.ValueObjects;

public record Barcode
{
    public string Value { get; }

    private Barcode(string value) => Value = value;

    public static Result<Barcode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Barcode>.Failure("Barcode is required.");

        var trimmed = value.Trim();

        if (trimmed.Length > 50)
            return Result<Barcode>.Failure("Barcode cannot exceed 50 characters.");

        return Result<Barcode>.Success(new Barcode(trimmed));
    }

    public static implicit operator string(Barcode barcode) => barcode.Value;
}
