namespace RetailOps.Core.Reporting.Application.DTOs;

public sealed record SalesReportRow(
    DateTime CompletedAt,
    string? CustomerName,
    string? SellerName,
    string PaymentMethod,
    string PaymentStatus,
    decimal Subtotal,
    decimal Discount,
    decimal Total);

public sealed record SalesReportDocument(
    string StoreName,
    string? LogoPath,
    DateTime From,
    DateTime To,
    IReadOnlyList<SalesReportRow> Rows,
    decimal GrandTotal);

public sealed record LowStockRow(
    string ProductCode,
    string ProductName,
    int CurrentStock,
    int MinimumLevel);

public sealed record LowStockReportDocument(
    string StoreName,
    string? LogoPath,
    IReadOnlyList<LowStockRow> Rows);

public sealed record CashSessionReportRow(
    DateTime OpenedAt,
    DateTime? ClosedAt,
    string TerminalName,
    string OperatorName,
    decimal OpeningFloat,
    decimal TotalSold,
    decimal? CountedCash,
    decimal? Breakage);

public sealed record CashSessionsReportDocument(
    string StoreName,
    string? LogoPath,
    DateTime From,
    DateTime To,
    IReadOnlyList<CashSessionReportRow> Rows);

public sealed record ProfitStatement(
    decimal TotalRevenue,
    decimal TotalCosts,
    decimal TotalExpenses,
    decimal NetProfit,
    bool IsEmpty);

public sealed record ProfitReportDocument(
    string StoreName,
    string? LogoPath,
    DateTime From,
    DateTime To,
    ProfitStatement Statement);

public sealed record ReceiptLine(
    string Barcode,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record ReceiptDocument(
    Guid SaleId,
    string StoreName,
    string? LogoPath,
    string? StoreAddress,
    string? StoreContacts,
    DateTime CompletedAt,
    string PaymentMethod,
    string PaymentTerms,
    string? CustomerName,
    IReadOnlyList<ReceiptLine> Lines,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    decimal AmountPaid,
    decimal ChangeAmount);

public sealed record StoreBrandingDto(
    string StoreName,
    string? LogoPath,
    string ReportFormat);
