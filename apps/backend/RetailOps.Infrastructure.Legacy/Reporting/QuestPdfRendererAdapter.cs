using System.Globalization;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RetailOps.Core.Reporting.Application.DTOs;
using RetailOps.Core.Reporting.Application.Ports;
using RetailOps.Core.Reporting.Application.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Reporting;

public sealed class QuestPdfRendererAdapter : IPdfRendererPort
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    static QuestPdfRendererAdapter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> RenderAsync<TModel>(string templateId, TModel model, ReportOutputFormat format)
    {
        if (format == ReportOutputFormat.Html)
            return Task.FromResult(RenderHtml(templateId, model));

        var bytes = templateId switch
        {
            "sales" when model is SalesReportDocument sales => RenderSalesPdf(sales),
            "low-stock" when model is LowStockReportDocument stock => RenderLowStockPdf(stock),
            "cash-sessions" when model is CashSessionsReportDocument sessions => RenderCashSessionsPdf(sessions),
            "profit" when model is ProfitReportDocument profit => RenderProfitPdf(profit),
            "receipt" when model is ReceiptDocument receipt => RenderReceiptPdf(receipt),
            _ => RenderFallbackPdf(templateId)
        };

        return Task.FromResult(bytes);
    }

    private static byte[] RenderHtml<TModel>(string templateId, TModel model)
    {
        var html = $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head><meta charset="utf-8"/><title>{templateId}</title></head>
            <body><h1>Relatório: {templateId}</h1><pre>{System.Net.WebUtility.HtmlEncode(model?.ToString() ?? "")}</pre></body>
            </html>
            """;
        return Encoding.UTF8.GetBytes(html);
    }

    private static byte[] RenderFallbackPdf(string templateId) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content().Text($"Template '{templateId}' not found.");
            });
        }).GeneratePdf();

    private static byte[] RenderSalesPdf(SalesReportDocument doc) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Element(c => ComposeHeader(c, doc.StoreName, "Relatório de Vendas",
                    $"{doc.From:d} — {doc.To:d}"));
                page.Content().Column(col =>
                {
                    col.Spacing(4);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Data").SemiBold();
                            header.Cell().Text("Cliente").SemiBold();
                            header.Cell().Text("Vendedor").SemiBold();
                            header.Cell().Text("Pagamento").SemiBold();
                            header.Cell().AlignRight().Text("Total").SemiBold();
                        });
                        foreach (var row in doc.Rows)
                        {
                            table.Cell().Text(row.CompletedAt.ToString("g", PtBr));
                            table.Cell().Text(row.CustomerName ?? "—");
                            table.Cell().Text(row.SellerName ?? "—");
                            table.Cell().Text($"{row.PaymentMethod} ({row.PaymentStatus})");
                            table.Cell().AlignRight().Text(FormatMoney(row.Total));
                        }
                    });
                    col.Item().PaddingTop(10).AlignRight().Text($"Total geral: {FormatMoney(doc.GrandTotal)}").SemiBold();
                });
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                });
            });
        }).GeneratePdf();

    private static byte[] RenderLowStockPdf(LowStockReportDocument doc) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Element(c => ComposeHeader(c, doc.StoreName, "Estoque Baixo", null));
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });
                    table.Header(header =>
                    {
                        header.Cell().Text("Código").SemiBold();
                        header.Cell().Text("Produto").SemiBold();
                        header.Cell().AlignRight().Text("Estoque").SemiBold();
                        header.Cell().AlignRight().Text("Mínimo").SemiBold();
                    });
                    foreach (var row in doc.Rows)
                    {
                        table.Cell().Text(row.ProductCode);
                        table.Cell().Text(row.ProductName);
                        table.Cell().AlignRight().Text(row.CurrentStock.ToString(PtBr));
                        table.Cell().AlignRight().Text(row.MinimumLevel.ToString(PtBr));
                    }
                });
            });
        }).GeneratePdf();

    private static byte[] RenderCashSessionsPdf(CashSessionsReportDocument doc) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Element(c => ComposeHeader(c, doc.StoreName, "Sessões de Caixa",
                    $"{doc.From:d} — {doc.To:d}"));
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });
                    table.Header(header =>
                    {
                        header.Cell().Text("Abertura").SemiBold();
                        header.Cell().Text("Terminal").SemiBold();
                        header.Cell().Text("Operador").SemiBold();
                        header.Cell().AlignRight().Text("Vendido").SemiBold();
                        header.Cell().AlignRight().Text("Contado").SemiBold();
                        header.Cell().AlignRight().Text("Quebra").SemiBold();
                    });
                    foreach (var row in doc.Rows)
                    {
                        table.Cell().Text(row.OpenedAt.ToString("g", PtBr));
                        table.Cell().Text(row.TerminalName);
                        table.Cell().Text(row.OperatorName);
                        table.Cell().AlignRight().Text(FormatMoney(row.TotalSold));
                        table.Cell().AlignRight().Text(row.CountedCash?.ToString("C", PtBr) ?? "—");
                        table.Cell().AlignRight().Text(row.Breakage?.ToString("C", PtBr) ?? "—");
                    }
                });
            });
        }).GeneratePdf();

    private static byte[] RenderProfitPdf(ProfitReportDocument doc) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));
                page.Header().Element(c => ComposeHeader(c, doc.StoreName, "Demonstrativo de Lucro",
                    $"{doc.From:d} — {doc.To:d}"));
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    if (doc.Statement.IsEmpty)
                        col.Item().Text("Nenhum movimento financeiro no período.").Italic();

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Receitas");
                        r.ConstantItem(120).AlignRight().Text(FormatMoney(doc.Statement.TotalRevenue));
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Custos");
                        r.ConstantItem(120).AlignRight().Text(FormatMoney(doc.Statement.TotalCosts));
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Despesas");
                        r.ConstantItem(120).AlignRight().Text(FormatMoney(doc.Statement.TotalExpenses));
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1);
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Lucro líquido").SemiBold();
                        r.ConstantItem(120).AlignRight().Text(FormatMoney(doc.Statement.NetProfit)).SemiBold();
                    });
                });
            });
        }).GeneratePdf();

    private static byte[] RenderReceiptPdf(ReceiptDocument doc) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(226.77f, 800);
                page.Margin(12);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Content().Column(col =>
                {
                    col.Spacing(4);
                    col.Item().AlignCenter().Text(doc.StoreName).SemiBold().FontSize(11);
                    if (!string.IsNullOrWhiteSpace(doc.StoreAddress))
                        col.Item().AlignCenter().Text(doc.StoreAddress!);
                    if (!string.IsNullOrWhiteSpace(doc.StoreContacts))
                        col.Item().AlignCenter().Text(doc.StoreContacts!);
                    col.Item().PaddingVertical(4).LineHorizontal(0.5f);
                    col.Item().Text($"Venda: {doc.CompletedAt:g}");
                    if (!string.IsNullOrWhiteSpace(doc.CustomerName))
                        col.Item().Text($"Cliente: {doc.CustomerName}");
                    col.Item().PaddingVertical(4).LineHorizontal(0.5f);
                    foreach (var line in doc.Lines)
                    {
                        col.Item().Text($"{line.Quantity}x {line.ProductName}");
                        col.Item().Text($"  {FormatMoney(line.UnitPrice)} = {FormatMoney(line.LineTotal)}");
                    }
                    col.Item().PaddingVertical(4).LineHorizontal(0.5f);
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal");
                        r.ConstantItem(70).AlignRight().Text(FormatMoney(doc.Subtotal));
                    });
                    if (doc.Discount > 0)
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Desconto");
                            r.ConstantItem(70).AlignRight().Text(FormatMoney(doc.Discount));
                        });
                    }
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Total").SemiBold();
                        r.ConstantItem(70).AlignRight().Text(FormatMoney(doc.Total)).SemiBold();
                    });
                    col.Item().Text($"Pagamento: {doc.PaymentMethod} ({doc.PaymentTerms})");
                    col.Item().Text($"Pago: {FormatMoney(doc.AmountPaid)}");
                    if (doc.ChangeAmount > 0)
                        col.Item().Text($"Troco: {FormatMoney(doc.ChangeAmount)}");
                });
            });
        }).GeneratePdf();

    private static void ComposeHeader(IContainer container, string storeName, string title, string? subtitle)
    {
        container.Column(col =>
        {
            col.Item().Text(storeName).SemiBold().FontSize(14);
            col.Item().Text(title).FontSize(12);
            if (!string.IsNullOrWhiteSpace(subtitle))
                col.Item().Text(subtitle!).FontSize(10);
            col.Item().PaddingBottom(8).LineHorizontal(1);
        });
    }

    private static string FormatMoney(decimal value) => value.ToString("C", PtBr);
}
