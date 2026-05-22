using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Domain.ValueObjects;

public readonly record struct PermissionKey(string Value)
{
    public static Result<PermissionKey> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<PermissionKey>.Failure("Permission key is required.");

        return Result<PermissionKey>.Success(new PermissionKey(value.Trim().ToLowerInvariant()));
    }
}
