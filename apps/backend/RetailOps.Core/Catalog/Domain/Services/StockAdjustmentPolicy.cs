using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.Services;

public sealed class StockAdjustmentPolicy
{
    public Result ValidateAdjustment(StockQuantity currentStock, StockQuantity newStock, bool isExit)
    {
        if (newStock.Value < 0)
            return Result.Failure("Stock quantity cannot be negative.");

        if (isExit && newStock.Value > currentStock.Value)
            return Result.Failure("Exit quantity cannot exceed current stock.");

        return Result.Success();
    }

    public Result ValidateExit(StockQuantity currentStock, StockQuantity exitQuantity)
    {
        if (exitQuantity.Value < 0)
            return Result.Failure("Exit quantity cannot be negative.");

        if (exitQuantity.Value > currentStock.Value)
            return Result.Failure("Exit quantity cannot exceed current stock.");

        return Result.Success();
    }
}
