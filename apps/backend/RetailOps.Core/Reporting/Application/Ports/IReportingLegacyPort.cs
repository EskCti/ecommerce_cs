using RetailOps.Core.Reporting.Application.DTOs;
using RetailOps.Core.Reporting.Application.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Reporting.Application.Ports;

public interface IReportingLegacyPort
{
    Task<Result<SalesReportDocument>> GetSalesReportAsync(
        TenantId tenantId,
        ReportFilter filter,
        StoreBrandingDto branding,
        CancellationToken ct = default);

    Task<Result<LowStockReportDocument>> GetLowStockReportAsync(
        TenantId tenantId,
        StoreBrandingDto branding,
        CancellationToken ct = default);

    Task<Result<CashSessionsReportDocument>> GetCashSessionsReportAsync(
        TenantId tenantId,
        ReportFilter filter,
        StoreBrandingDto branding,
        CancellationToken ct = default);

    Task<Result<ProfitReportDocument>> GetProfitReportAsync(
        TenantId tenantId,
        ReportFilter filter,
        StoreBrandingDto branding,
        CancellationToken ct = default);

    Task<Result<ReceiptDocument>> GetReceiptAsync(
        TenantId tenantId,
        Guid saleId,
        StoreBrandingDto branding,
        CancellationToken ct = default);
}
