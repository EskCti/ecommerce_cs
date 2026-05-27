using RetailOps.Core.Catalog.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Domain.Services;

public static class LowStockPolicy
{
    public static bool IsLowStock(StockQuantity stock, StockAlertLevel alertLevel) =>
        stock.Value < alertLevel.Value;

    public static bool ShouldEmitLowStockDetected(StockQuantity stock, StockAlertLevel alertLevel) =>
        IsLowStock(stock, alertLevel);
}
