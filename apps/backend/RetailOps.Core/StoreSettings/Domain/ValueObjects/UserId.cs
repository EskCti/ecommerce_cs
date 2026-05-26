using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public readonly record struct UserId(Guid Value)
{
    public static UserId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public static Result<UserId> Create(Guid value)
    {
        if (value == Guid.Empty)
            return Result<UserId>.Failure("User id cannot be empty.");

        return Result<UserId>.Success(new UserId(value));
    }

    public static Result<UserId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<UserId>.Failure("User id is required.");

        if (!Guid.TryParse(value, out var guid))
            return Result<UserId>.Failure("Invalid user id format.");

        return Create(guid);
    }
}