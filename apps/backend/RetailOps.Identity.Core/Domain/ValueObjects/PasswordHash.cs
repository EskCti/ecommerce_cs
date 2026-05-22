using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Domain.ValueObjects;

public readonly record struct PasswordHash(string Value)
{
    public static Result<PasswordHash> CreateBcrypt(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return Result<PasswordHash>.Failure("Password hash is required.");

        return Result<PasswordHash>.Success(new PasswordHash(hash));
    }
}
