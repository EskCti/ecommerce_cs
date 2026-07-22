using RetailOps.Core.Reporting.Application.DTOs;
using RetailOps.Core.Reporting.Application.Ports;
using RetailOps.Core.Reporting.Application.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Reporting.Application.Queries;

public interface IReportSalesQuery
{
    Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        ReportFilter filter,
        ReportOutputFormat format,
        CancellationToken ct = default);
}

public sealed class ReportSalesQuery(
    IReportingLegacyPort legacyPort,
    IReportingStoreBrandingPort brandingPort,
    IPdfRendererPort pdfRenderer) : IReportSalesQuery
{
    public async Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        ReportFilter filter,
        ReportOutputFormat format,
        CancellationToken ct = default)
    {
        var brandingResult = await brandingPort.GetBrandingAsync(tenantId, ct);
        if (brandingResult.IsFailure)
            return Result<byte[]>.Failure(brandingResult.Error);

        var reportResult = await legacyPort.GetSalesReportAsync(tenantId, filter, brandingResult.Value, ct);
        if (reportResult.IsFailure)
            return Result<byte[]>.Failure(reportResult.Error);

        var bytes = await pdfRenderer.RenderAsync("sales", reportResult.Value, format);
        return Result<byte[]>.Success(bytes);
    }
}

public interface IReportLowStockQuery
{
    Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        ReportOutputFormat format,
        CancellationToken ct = default);
}

public sealed class ReportLowStockQuery(
    IReportingLegacyPort legacyPort,
    IReportingStoreBrandingPort brandingPort,
    IPdfRendererPort pdfRenderer) : IReportLowStockQuery
{
    public async Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        ReportOutputFormat format,
        CancellationToken ct = default)
    {
        var brandingResult = await brandingPort.GetBrandingAsync(tenantId, ct);
        if (brandingResult.IsFailure)
            return Result<byte[]>.Failure(brandingResult.Error);

        var reportResult = await legacyPort.GetLowStockReportAsync(tenantId, brandingResult.Value, ct);
        if (reportResult.IsFailure)
            return Result<byte[]>.Failure(reportResult.Error);

        var bytes = await pdfRenderer.RenderAsync("low-stock", reportResult.Value, format);
        return Result<byte[]>.Success(bytes);
    }
}

public interface IReportCashSessionsQuery
{
    Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        ReportFilter filter,
        ReportOutputFormat format,
        CancellationToken ct = default);
}

public sealed class ReportCashSessionsQuery(
    IReportingLegacyPort legacyPort,
    IReportingStoreBrandingPort brandingPort,
    IPdfRendererPort pdfRenderer) : IReportCashSessionsQuery
{
    public async Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        ReportFilter filter,
        ReportOutputFormat format,
        CancellationToken ct = default)
    {
        var brandingResult = await brandingPort.GetBrandingAsync(tenantId, ct);
        if (brandingResult.IsFailure)
            return Result<byte[]>.Failure(brandingResult.Error);

        var reportResult = await legacyPort.GetCashSessionsReportAsync(tenantId, filter, brandingResult.Value, ct);
        if (reportResult.IsFailure)
            return Result<byte[]>.Failure(reportResult.Error);

        var bytes = await pdfRenderer.RenderAsync("cash-sessions", reportResult.Value, format);
        return Result<byte[]>.Success(bytes);
    }
}

public interface IReportProfitQuery
{
    Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        ReportFilter filter,
        ReportOutputFormat format,
        CancellationToken ct = default);
}

public sealed class ReportProfitQuery(
    IReportingLegacyPort legacyPort,
    IReportingStoreBrandingPort brandingPort,
    IPdfRendererPort pdfRenderer) : IReportProfitQuery
{
    public async Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        ReportFilter filter,
        ReportOutputFormat format,
        CancellationToken ct = default)
    {
        var brandingResult = await brandingPort.GetBrandingAsync(tenantId, ct);
        if (brandingResult.IsFailure)
            return Result<byte[]>.Failure(brandingResult.Error);

        var reportResult = await legacyPort.GetProfitReportAsync(tenantId, filter, brandingResult.Value, ct);
        if (reportResult.IsFailure)
            return Result<byte[]>.Failure(reportResult.Error);

        var bytes = await pdfRenderer.RenderAsync("profit", reportResult.Value, format);
        return Result<byte[]>.Success(bytes);
    }
}

public interface IGenerateReceiptQuery
{
    Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        Guid saleId,
        ReportOutputFormat format,
        CancellationToken ct = default);
}

public sealed class GenerateReceiptQuery(
    IReportingLegacyPort legacyPort,
    IReportingStoreBrandingPort brandingPort,
    IPdfRendererPort pdfRenderer) : IGenerateReceiptQuery
{
    public async Task<Result<byte[]>> ExecuteAsync(
        TenantId tenantId,
        Guid saleId,
        ReportOutputFormat format,
        CancellationToken ct = default)
    {
        var brandingResult = await brandingPort.GetBrandingAsync(tenantId, ct);
        if (brandingResult.IsFailure)
            return Result<byte[]>.Failure(brandingResult.Error);

        var reportResult = await legacyPort.GetReceiptAsync(tenantId, saleId, brandingResult.Value, ct);
        if (reportResult.IsFailure)
            return Result<byte[]>.Failure(reportResult.Error);

        var bytes = await pdfRenderer.RenderAsync("receipt", reportResult.Value, format);
        return Result<byte[]>.Success(bytes);
    }
}
