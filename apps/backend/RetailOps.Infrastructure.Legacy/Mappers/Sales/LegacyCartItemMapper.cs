using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Infrastructure.Legacy.Mappers.Sales;

internal static class LegacyCartItemMapper
{
    public static Result<SaleLine> ToDomain(LegacyCartItemRow row)
    {
        var unitPriceResult = UnitPrice.Create(row.UnitPrice);
        if (unitPriceResult.IsFailure)
            return Result<SaleLine>.Failure(unitPriceResult.Error);

        var gradeIds = ParseGradeOptionIds(row.GradeOptionIds);
        var status = row.Status.Equals("Pronto", StringComparison.OrdinalIgnoreCase)
            ? SaleLineStatus.Ready
            : SaleLineStatus.PendingGrade;

        var requiresGrade = status == SaleLineStatus.PendingGrade;

        return SaleLine.Reconstitute(
            LegacySalesIds.CartLine(row.Id),
            Mappers.Catalog.LegacyCatalogIds.Product(row.ProductLegacyId),
            row.Barcode,
            row.Quantity,
            unitPriceResult.Value,
            requiresGrade,
            status,
            gradeIds);
    }

    public static LegacyCartItemRow ToRow(SaleLine line, CashSession session, int cashSessionLegacyId, int? legacyId = null)
    {
        var productLegacyId = LegacySalesIds.ParseProductLegacyId(line.ProductId) ?? 0;

        return new LegacyCartItemRow
        {
            Id = legacyId ?? LegacySalesIds.ParseCartLineLegacyId(line.Id) ?? 0,
            CompanyId = session.TenantId.Value,
            CashSessionLegacyId = cashSessionLegacyId,
            ProductLegacyId = productLegacyId,
            Barcode = line.Barcode,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice.Value,
            SaleLegacyId = 0,
            GradeOptionIds = FormatGradeOptionIds(line.GradeOptionIds),
            Status = line.Status == SaleLineStatus.Ready ? "Pronto" : "PendenteGrade",
        };
    }

    private static List<Guid> ParseGradeOptionIds(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Guid.TryParse(s, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();
    }

    private static string? FormatGradeOptionIds(IEnumerable<Guid> ids)
    {
        var list = ids.ToList();
        return list.Count == 0 ? null : string.Join(',', list);
    }
}
