using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Shared.Kernel.Domain.ValueObjects;

public readonly record struct TenantId(int Value)
{
    public static TenantId Platform => new(0);

    public bool IsPlatform => Value == 0;

    public static Result<TenantId> Create(int value)
    {
        if (value < 0)
            return Result<TenantId>.Failure("Tenant id cannot be negative.");

        return Result<TenantId>.Success(new TenantId(value));
    }
}
