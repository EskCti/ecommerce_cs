using RetailOps.Core.Reporting.Application.DTOs;
using RetailOps.Infrastructure.Legacy.Mappers.Finance;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;

namespace RetailOps.Infrastructure.Legacy.Reporting;

internal static class LegacyReportMapper
{
    public static SalesReportRow ToSalesRow(
        LegacyReceivableRow row,
        string? customerName,
        string? sellerName,
        string? paymentMethodName)
    {
        var status = string.Equals(row.Paid, "Sim", StringComparison.OrdinalIgnoreCase) ? "Pago" : "Aberto";
        var subtotal = row.Subtotal ?? row.Amount;
        var discount = row.Discount ?? 0m;

        return new SalesReportRow(
            row.CompletedAt ?? row.DueDate,
            customerName,
            sellerName,
            paymentMethodName ?? "—",
            status,
            subtotal,
            discount,
            row.Amount);
    }

    public static LowStockRow ToLowStockRow(LegacyProductRow row) =>
        new(
            row.Code ?? row.Id.ToString(),
            row.Name ?? "—",
            row.Stock,
            row.StockAlertLevel);

    public static CashSessionReportRow ToCashSessionRow(
        LegacyCashSessionRow row,
        string terminalName,
        string operatorName) =>
        new(
            row.OpenedAt,
            row.ClosedAt,
            terminalName,
            operatorName,
            row.OpeningFloat,
            row.TotalSold,
            row.CountedCash,
            row.Breakage);

    public static ReceiptLine ToReceiptLine(LegacyCartItemRow row, string productName) =>
        new(
            row.Barcode,
            productName,
            row.Quantity,
            row.UnitPrice,
            row.UnitPrice * row.Quantity);
}
