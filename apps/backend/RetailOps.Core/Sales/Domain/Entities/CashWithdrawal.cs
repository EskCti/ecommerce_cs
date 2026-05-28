using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.Entities;

public sealed class CashWithdrawal : Entity
{
    public decimal Amount { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private CashWithdrawal() { }

    private CashWithdrawal(decimal amount, DateTime registeredAt)
    {
        Amount = amount;
        RegisteredAt = registeredAt;
    }

    public static Result<CashWithdrawal> Create(decimal amount)
    {
        if (amount <= 0)
            return Result<CashWithdrawal>.Failure("Withdrawal amount must be greater than zero.");

        return Result<CashWithdrawal>.Success(new CashWithdrawal(Math.Round(amount, 2), DateTime.UtcNow));
    }

    public static Result<CashWithdrawal> Reconstitute(Guid id, decimal amount, DateTime registeredAt)
    {
        var withdrawal = new CashWithdrawal(amount, registeredAt) { Id = id };
        return Result<CashWithdrawal>.Success(withdrawal);
    }
}
