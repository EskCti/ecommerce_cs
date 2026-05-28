namespace RetailOps.Core.Sales.Application.Ports;

public interface ISaleParallelRunLogger
{
    void LogComparison(
        int tenantId,
        Guid cashSessionId,
        Guid saleId,
        decimal csharpTotal,
        decimal legacyTotal);
}
