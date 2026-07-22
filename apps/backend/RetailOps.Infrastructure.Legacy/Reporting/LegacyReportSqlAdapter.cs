using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Reporting.Application.DTOs;
using RetailOps.Core.Reporting.Application.Ports;
using RetailOps.Core.Reporting.Application.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.Finance;
using RetailOps.Infrastructure.Legacy.Mappers.Sales;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Reporting;

public sealed class LegacyReportSqlAdapter(LegacySasDbContext db) : IReportingLegacyPort
{
    public async Task<Result<SalesReportDocument>> GetSalesReportAsync(
        TenantId tenantId,
        ReportFilter filter,
        StoreBrandingDto branding,
        CancellationToken ct = default)
    {
        var companyId = tenantId.Value;
        var from = filter.DateRange.From;
        var to = filter.DateRange.To;

        var query = db.Receivables.AsNoTracking()
            .Where(r => r.CompanyId == companyId)
            .Where(r => r.Type != null && r.Type.StartsWith("Venda"))
            .Where(r => r.CompletedAt != null && r.CompletedAt >= from && r.CompletedAt <= to);

        if (filter.CustomerId is Guid customerId)
        {
            var customerLegacyId = LegacySalesIds.ParseCustomerLegacyId(customerId);
            if (customerLegacyId is null)
                return Result<SalesReportDocument>.Failure("Invalid customer filter.");
            query = query.Where(r => r.CustomerLegacyId == customerLegacyId);
        }

        if (filter.SellerId is Guid sellerId)
        {
            var sellerLegacyId = LegacySalesIds.ParseOperatorLegacyId(sellerId);
            if (sellerLegacyId is null)
                return Result<SalesReportDocument>.Failure("Invalid seller filter.");
            query = query.Where(r => r.OperatorLegacyUserId == sellerLegacyId);
        }

        if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
        {
            var paid = filter.PaymentStatus.Equals("Settled", StringComparison.OrdinalIgnoreCase)
                       || filter.PaymentStatus.Equals("Pago", StringComparison.OrdinalIgnoreCase);
            query = paid
                ? query.Where(r => r.Paid == "Sim")
                : query.Where(r => r.Paid != "Sim");
        }

        var rows = await query.OrderByDescending(r => r.CompletedAt).ToListAsync(ct);

        var customerIds = rows.Where(r => r.CustomerLegacyId > 0).Select(r => r.CustomerLegacyId!.Value).Distinct().ToList();
        var sellerIds = rows.Where(r => r.OperatorLegacyUserId > 0).Select(r => r.OperatorLegacyUserId!.Value).Distinct().ToList();
        var paymentIds = rows.Where(r => r.PaymentMethodLegacyId > 0).Select(r => r.PaymentMethodLegacyId!.Value).Distinct().ToList();

        var customers = await db.Customers.AsNoTracking()
            .Where(c => c.CompanyId == companyId && customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Nome ?? "—", ct);

        var sellers = await db.Users.AsNoTracking()
            .Where(u => u.CompanyId == companyId && sellerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name ?? "—", ct);

        var payments = await db.PaymentMethods.AsNoTracking()
            .Where(p => p.CompanyId == companyId && paymentIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Nome ?? "—", ct);

        var mapped = rows.Select(row =>
        {
            var customerName = row.CustomerLegacyId is > 0 && customers.TryGetValue(row.CustomerLegacyId.Value, out var cn)
                ? cn
                : null;
            var sellerName = row.OperatorLegacyUserId is > 0 && sellers.TryGetValue(row.OperatorLegacyUserId.Value, out var sn)
                ? sn
                : null;
            var paymentName = row.PaymentMethodLegacyId is > 0 && payments.TryGetValue(row.PaymentMethodLegacyId.Value, out var pn)
                ? pn
                : null;
            return LegacyReportMapper.ToSalesRow(row, customerName, sellerName, paymentName);
        }).ToList();

        return Result<SalesReportDocument>.Success(new SalesReportDocument(
            branding.StoreName,
            branding.LogoPath,
            from,
            to,
            mapped,
            mapped.Sum(r => r.Total)));
    }

    public async Task<Result<LowStockReportDocument>> GetLowStockReportAsync(
        TenantId tenantId,
        StoreBrandingDto branding,
        CancellationToken ct = default)
    {
        var companyId = tenantId.Value;

        var products = await db.Products.AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .Where(p => p.Active == "Sim" || p.Active == null)
            .Where(p => p.Stock <= p.StockAlertLevel)
            .OrderBy(p => p.Stock)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);

        var rows = products.Select(LegacyReportMapper.ToLowStockRow).ToList();

        return Result<LowStockReportDocument>.Success(new LowStockReportDocument(
            branding.StoreName,
            branding.LogoPath,
            rows));
    }

    public async Task<Result<CashSessionsReportDocument>> GetCashSessionsReportAsync(
        TenantId tenantId,
        ReportFilter filter,
        StoreBrandingDto branding,
        CancellationToken ct = default)
    {
        var companyId = tenantId.Value;
        var from = filter.DateRange.From;
        var to = filter.DateRange.To;

        var sessions = await db.CashSessions.AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .Where(s => s.OpenedAt >= from && s.OpenedAt <= to)
            .OrderByDescending(s => s.OpenedAt)
            .ToListAsync(ct);

        var terminalIds = sessions.Select(s => s.TerminalLegacyId).Distinct().ToList();
        var operatorIds = sessions.Select(s => s.OperatorLegacyUserId).Distinct().ToList();

        var terminals = await db.CashRegisters.AsNoTracking()
            .Where(t => t.CompanyId == companyId && terminalIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Nome ?? "—", ct);

        var operators = await db.Users.AsNoTracking()
            .Where(u => u.CompanyId == companyId && operatorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name ?? "—", ct);

        var rows = sessions.Select(session =>
        {
            terminals.TryGetValue(session.TerminalLegacyId, out var terminalName);
            operators.TryGetValue(session.OperatorLegacyUserId, out var operatorName);
            return LegacyReportMapper.ToCashSessionRow(session, terminalName ?? "—", operatorName ?? "—");
        }).ToList();

        return Result<CashSessionsReportDocument>.Success(new CashSessionsReportDocument(
            branding.StoreName,
            branding.LogoPath,
            from,
            to,
            rows));
    }

    public async Task<Result<ProfitReportDocument>> GetProfitReportAsync(
        TenantId tenantId,
        ReportFilter filter,
        StoreBrandingDto branding,
        CancellationToken ct = default)
    {
        var companyId = tenantId.Value;
        var from = filter.DateRange.From;
        var to = filter.DateRange.To;

        var sales = await db.Receivables.AsNoTracking()
            .Where(r => r.CompanyId == companyId)
            .Where(r => r.Type != null && r.Type.StartsWith("Venda"))
            .Where(r => r.CompletedAt != null && r.CompletedAt >= from && r.CompletedAt <= to)
            .Where(r => r.Cancelled != "Sim")
            .ToListAsync(ct);

        var totalRevenue = sales.Sum(s => s.Amount);

        var saleIds = sales.Select(s => s.Id).ToList();
        var lineCosts = await db.CartItems.AsNoTracking()
            .Where(i => i.CompanyId == companyId && saleIds.Contains(i.SaleLegacyId))
            .Join(
                db.Products.AsNoTracking().Where(p => p.CompanyId == companyId),
                item => item.ProductLegacyId,
                product => product.Id,
                (item, product) => item.Quantity * product.CostPrice)
            .SumAsync(ct);

        var expenses = await db.Payables.AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .Where(p => p.DueDate >= from && p.DueDate <= to)
            .Where(p => p.Paid == "Sim")
            .SumAsync(p => p.Amount, ct);

        var totalCosts = lineCosts;
        var netProfit = totalRevenue - totalCosts - expenses;
        var isEmpty = totalRevenue == 0 && totalCosts == 0 && expenses == 0;

        return Result<ProfitReportDocument>.Success(new ProfitReportDocument(
            branding.StoreName,
            branding.LogoPath,
            from,
            to,
            new ProfitStatement(totalRevenue, totalCosts, expenses, netProfit, isEmpty)));
    }

    public async Task<Result<ReceiptDocument>> GetReceiptAsync(
        TenantId tenantId,
        Guid saleId,
        StoreBrandingDto branding,
        CancellationToken ct = default)
    {
        var saleLegacyId = LegacySalesIds.ParseSaleLegacyId(saleId);
        if (saleLegacyId is null)
            return Result<ReceiptDocument>.Failure("Invalid sale id.");

        var companyId = tenantId.Value;

        var saleRow = await db.Receivables.AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.CompanyId == companyId && r.Id == saleLegacyId.Value,
                ct);

        if (saleRow is null || LegacyReceivableMapper.ResolveType(saleRow.Type) != LegacyReceivableType.Venda)
            return Result<ReceiptDocument>.Failure("Sale not found.");

        var lines = await db.CartItems.AsNoTracking()
            .Where(i => i.CompanyId == companyId && i.SaleLegacyId == saleLegacyId.Value)
            .ToListAsync(ct);

        var productIds = lines.Select(l => l.ProductLegacyId).Distinct().ToList();
        var products = await db.Products.AsNoTracking()
            .Where(p => p.CompanyId == companyId && productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name ?? "—", ct);

        string? customerName = null;
        if (saleRow.CustomerLegacyId is > 0)
        {
            customerName = await db.Customers.AsNoTracking()
                .Where(c => c.CompanyId == companyId && c.Id == saleRow.CustomerLegacyId)
                .Select(c => c.Nome)
                .FirstOrDefaultAsync(ct);
        }

        string paymentMethod = "—";
        if (saleRow.PaymentMethodLegacyId is > 0)
        {
            paymentMethod = await db.PaymentMethods.AsNoTracking()
                .Where(p => p.CompanyId == companyId && p.Id == saleRow.PaymentMethodLegacyId)
                .Select(p => p.Nome)
                .FirstOrDefaultAsync(ct) ?? "—";
        }

        var config = await db.Configs.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyId == companyId, ct);

        var receiptLines = lines.Select(line =>
        {
            products.TryGetValue(line.ProductLegacyId, out var productName);
            return LegacyReportMapper.ToReceiptLine(line, productName ?? "—");
        }).ToList();

        var subtotal = saleRow.Subtotal ?? receiptLines.Sum(l => l.LineTotal);
        var discount = saleRow.Discount ?? 0m;
        var total = saleRow.Amount;
        var change = saleRow.ChangeAmount ?? 0m;
        var amountPaid = total + change;
        var paymentTerms = string.Equals(saleRow.Paid, "Sim", StringComparison.OrdinalIgnoreCase) ? "À vista" : "Fiado";

        return Result<ReceiptDocument>.Success(new ReceiptDocument(
            saleId,
            branding.StoreName,
            branding.LogoPath,
            config?.Endereco,
            config?.Contatos,
            saleRow.CompletedAt ?? saleRow.DueDate,
            paymentMethod,
            paymentTerms,
            customerName,
            receiptLines,
            subtotal,
            discount,
            total,
            amountPaid,
            change));
    }
}
