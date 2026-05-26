using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public record SurchargePercent
{
    public decimal Value { get; }

    private SurchargePercent(decimal value) => Value = value;

    public static Result<SurchargePercent> Create(decimal value)
    {
        if (value < 0)
            return Result<SurchargePercent>.Failure("Surcharge percent cannot be negative.");
        
        if (value > 100)
            return Result<SurchargePercent>.Failure("Surcharge percent cannot exceed 100%.");

        return Result<SurchargePercent>.Success(new SurchargePercent(value));
    }

    public static implicit operator decimal(SurchargePercent surchargePercent) => surchargePercent.Value;
}