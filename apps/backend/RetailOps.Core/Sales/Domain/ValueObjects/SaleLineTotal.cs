using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.ValueObjects;

public record SaleLineTotal
{
    public decimal Value { get; }

    private SaleLineTotal(decimal value) => Value = value;

    public static Result<SaleLineTotal> FromQuantityAndUnitPrice(int quantity, UnitPrice unitPrice)
    {
        if (quantity <= 0)
            return Result<SaleLineTotal>.Failure("Quantity must be greater than zero.");

        return Result<SaleLineTotal>.Success(new SaleLineTotal(Math.Round(quantity * unitPrice.Value, 2)));
    }
}
