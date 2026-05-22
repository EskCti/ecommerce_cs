using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Domain.ValueObjects;

public readonly record struct ManagerPin(string BcryptHash)
{
    public static Result<ManagerPin> FromBcrypt(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return Result<ManagerPin>.Failure("Manager PIN hash is required.");

        return Result<ManagerPin>.Success(new ManagerPin(hash));
    }
}
