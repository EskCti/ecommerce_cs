using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.ValueObjects;

public record ProductName
{
    public string Value { get; }

    private ProductName(string value) => Value = value;

    public static Result<ProductName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<ProductName>.Failure("Product name is required.");

        var trimmed = value.Trim();

        if (trimmed.Length > 100)
            return Result<ProductName>.Failure("Product name cannot exceed 100 characters.");

        return Result<ProductName>.Success(new ProductName(trimmed));
    }

    public static implicit operator string(ProductName name) => name.Value;
}
