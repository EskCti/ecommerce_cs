using Microsoft.Extensions.Logging;
using RetailOps.Core.Sales.Application.Ports;

namespace RetailOps.Infrastructure.Legacy.Sales;

public sealed class SaleParallelRunLogger(ILogger<SaleParallelRunLogger> logger) : ISaleParallelRunLogger
{
    private const decimal AlertThresholdPercent = 0.1m;

    public void LogComparison(
        int tenantId,
        Guid cashSessionId,
        Guid saleId,
        decimal csharpTotal,
        decimal legacyTotal)
    {
        var difference = Math.Abs(csharpTotal - legacyTotal);
        var baseTotal = legacyTotal == 0 ? 1 : legacyTotal;
        var percent = difference / baseTotal * 100m;

        if (percent > AlertThresholdPercent)
        {
            logger.LogWarning(
                "Sale parallel run divergence {Percent:F4}% tenant={TenantId} session={SessionId} sale={SaleId} csharp={CSharpTotal} legacy={LegacyTotal}",
                percent,
                tenantId,
                cashSessionId,
                saleId,
                csharpTotal,
                legacyTotal);
        }
        else
        {
            logger.LogInformation(
                "Sale parallel run within tolerance tenant={TenantId} session={SessionId} sale={SaleId} csharp={CSharpTotal} legacy={LegacyTotal}",
                tenantId,
                cashSessionId,
                saleId,
                csharpTotal,
                legacyTotal);
        }
    }
}
