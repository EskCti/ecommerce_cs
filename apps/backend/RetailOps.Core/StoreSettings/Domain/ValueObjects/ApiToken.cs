using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public record ApiToken
{
    public string Value { get; }

    private ApiToken(string value) => Value = value;

    public static Result<ApiToken> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<ApiToken>.Failure("API token is required.");

        var trimmed = value.Trim();
        
        if (trimmed.Length < 10)
            return Result<ApiToken>.Failure("API token must be at least 10 characters long.");
        
        if (trimmed.Length > 255)
            return Result<ApiToken>.Failure("API token cannot exceed 255 characters.");

        return Result<ApiToken>.Success(new ApiToken(trimmed));
    }

    public static implicit operator string(ApiToken apiToken) => apiToken.Value;
}