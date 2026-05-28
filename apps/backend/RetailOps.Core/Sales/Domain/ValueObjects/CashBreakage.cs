using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.ValueObjects;

public record CashBreakage
{
    public decimal Value { get; }

    private CashBreakage(decimal value) => Value = value;

    public static Result<CashBreakage> Create(decimal value) =>
        Result<CashBreakage>.Success(new CashBreakage(Math.Round(value, 2)));
}
