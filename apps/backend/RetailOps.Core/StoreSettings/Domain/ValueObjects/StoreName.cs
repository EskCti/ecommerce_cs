using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public record StoreName
{
    public string Value { get; }

    private StoreName(string value) => Value = value;

    public static Result<StoreName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<StoreName>.Failure("Store name is required.");

        var trimmed = value.Trim();
        
        if (trimmed.Length < 3)
            return Result<StoreName>.Failure("Store name must be at least 3 characters long.");
        
        if (trimmed.Length > 100)
            return Result<StoreName>.Failure("Store name cannot exceed 100 characters.");

        return Result<StoreName>.Success(new StoreName(trimmed));
    }

    public static implicit operator string(StoreName storeName) => storeName.Value;
}