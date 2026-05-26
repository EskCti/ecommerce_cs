using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public record CommissionRate
{
    public decimal Value { get; }

    private CommissionRate(decimal value) => Value = value;

    public static Result<CommissionRate> Create(decimal value)
    {
        if (value < 0)
            return Result<CommissionRate>.Failure("Commission rate cannot be negative.");
        
        if (value > 100)
            return Result<CommissionRate>.Failure("Commission rate cannot exceed 100%.");

        return Result<CommissionRate>.Success(new CommissionRate(value));
    }

    public static implicit operator decimal(CommissionRate commissionRate) => commissionRate.Value;
}