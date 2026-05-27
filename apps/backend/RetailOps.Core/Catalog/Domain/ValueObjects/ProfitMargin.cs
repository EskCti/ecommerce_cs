using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.ValueObjects;

public record ProfitMargin
{
    public decimal Value { get; }

    private ProfitMargin(decimal value) => Value = value;

    public static Result<ProfitMargin> Create(decimal value) =>
        Result<ProfitMargin>.Success(new ProfitMargin(value));
}
