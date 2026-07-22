using RetailOps.Core.Sales.Application.Ports;

namespace RetailOps.Infrastructure.Cutover;

public sealed class NullSaleParallelRunLogger : ISaleParallelRunLogger
{
    public void LogComparison(
        int tenantId,
        Guid cashSessionId,
        Guid saleId,
        decimal csharpTotal,
        decimal legacyTotal)
    {
    }
}
