using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Shared.Kernel.Domain.ValueObjects;

public record Money(decimal Amount, string Currency)
{
    public static Result<Money> Create(decimal amount, string currency = "BRL")
    {
        if (amount < 0)
            return Result<Money>.Failure("Amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return Result<Money>.Failure("Currency must be a 3-letter ISO code.");

        return Result<Money>.Success(new Money(amount, currency.Trim().ToUpperInvariant()));
    }
}
