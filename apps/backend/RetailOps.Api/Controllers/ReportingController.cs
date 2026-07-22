using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.MultiTenancy;
using RetailOps.Core.Reporting.Application.Queries;
using RetailOps.Core.Reporting.Application.ValueObjects;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/reporting")]
[Authorize]
public sealed class ReportingController(
    ITenantContext tenantContext,
    IReportSalesQuery reportSalesQuery,
    IReportLowStockQuery reportLowStockQuery,
    IReportCashSessionsQuery reportCashSessionsQuery,
    IReportProfitQuery reportProfitQuery,
    IGenerateReceiptQuery generateReceiptQuery) : ControllerBase
{
    [HttpGet("sales")]
    public Task<IActionResult> Sales(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? sellerId,
        [FromQuery] string? status,
        [FromQuery] string? format,
        CancellationToken ct)
        => ExportWithDateFilter(
            from,
            to,
            customerId,
            sellerId,
            status,
            format,
            (filter, outputFormat) => reportSalesQuery.ExecuteAsync(tenantContext.TenantId, filter, outputFormat, ct));

    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock([FromQuery] string? format, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var outputFormat = ParseFormat(format);
        var result = await reportLowStockQuery.ExecuteAsync(tenantContext.TenantId, outputFormat, ct);
        return ToFileResult(result, outputFormat, "estoque-baixo");
    }

    [HttpGet("cash-sessions")]
    public Task<IActionResult> CashSessions(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? format,
        CancellationToken ct)
        => ExportWithDateFilter(
            from,
            to,
            null,
            null,
            null,
            format,
            (filter, outputFormat) => reportCashSessionsQuery.ExecuteAsync(tenantContext.TenantId, filter, outputFormat, ct));

    [HttpGet("profit")]
    public Task<IActionResult> Profit(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? format,
        CancellationToken ct)
        => ExportWithDateFilter(
            from,
            to,
            null,
            null,
            null,
            format,
            (filter, outputFormat) => reportProfitQuery.ExecuteAsync(tenantContext.TenantId, filter, outputFormat, ct));

    [HttpGet("receipts/{saleId:guid}")]
    public async Task<IActionResult> Receipt(Guid saleId, [FromQuery] string? format, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var outputFormat = ParseFormat(format);
        var result = await generateReceiptQuery.ExecuteAsync(tenantContext.TenantId, saleId, outputFormat, ct);
        if (result.IsFailure)
        {
            return result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return ToFileResult(result, outputFormat, $"recibo-{saleId:N}");
    }

    private async Task<IActionResult> ExportWithDateFilter(
        DateTime from,
        DateTime to,
        Guid? customerId,
        Guid? sellerId,
        string? status,
        string? format,
        Func<ReportFilter, ReportOutputFormat, Task<RetailOps.Shared.Kernel.Domain.Results.Result<byte[]>>> execute)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filterResult = ReportFilter.Create(from, to, customerId, sellerId, status);
        if (filterResult.IsFailure)
            return BadRequest(new { error = filterResult.Error });

        var outputFormat = ParseFormat(format);
        var result = await execute(filterResult.Value, outputFormat);
        return ToFileResult(result, outputFormat, "relatorio");
    }

    private static ReportOutputFormat ParseFormat(string? format) =>
        string.Equals(format, "html", StringComparison.OrdinalIgnoreCase)
            ? ReportOutputFormat.Html
            : ReportOutputFormat.Pdf;

    private IActionResult ToFileResult(
        RetailOps.Shared.Kernel.Domain.Results.Result<byte[]> result,
        ReportOutputFormat format,
        string fileName)
    {
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        if (format == ReportOutputFormat.Html)
            return File(result.Value, "text/html", $"{fileName}.html");

        return File(result.Value, "application/pdf", $"{fileName}.pdf");
    }
}
