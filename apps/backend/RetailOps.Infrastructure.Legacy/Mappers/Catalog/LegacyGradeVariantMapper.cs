using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Infrastructure.Legacy.Mappers.Catalog;

public static class LegacyGradeVariantMapper
{
    public static Result<GradeVariant> ToDomain(
        LegacyGradeVariantRow row,
        Guid productId,
        IReadOnlyList<GradeDimension> dimensions)
    {
        var optionIds = new List<Guid>();
        if (row.OptionLegacyId1 is int option1)
            optionIds.Add(LegacyCatalogIds.GradeOption(option1));

        if (row.OptionLegacyId2 is int option2)
            optionIds.Add(LegacyCatalogIds.GradeOption(option2));

        var stockResult = StockQuantity.Create(row.Stock);
        if (stockResult.IsFailure)
            return Result<GradeVariant>.Failure(stockResult.Error);

        return GradeVariant.Reconstitute(
            LegacyCatalogIds.GradeVariant(row.Id),
            productId,
            optionIds,
            stockResult.Value);
    }

    public static Result<LegacyGradeVariantRow> ToLegacy(
        GradeVariant variant,
        int productLegacyId,
        IReadOnlyList<GradeDimension> dimensions,
        int? legacyId = null)
    {
        int? option1 = null;
        int? option2 = null;

        foreach (var optionId in variant.OptionIds)
        {
            var parsed = LegacyCatalogIds.ParseLegacyId(optionId, "0012");
            if (parsed is null)
                continue;

            if (option1 is null)
                option1 = parsed;
            else
                option2 = parsed;
        }

        var row = new LegacyGradeVariantRow
        {
            Id = legacyId ?? LegacyCatalogIds.ParseLegacyId(variant.Id, "0015") ?? 0,
            ProductLegacyId = productLegacyId,
            OptionLegacyId1 = option1,
            OptionLegacyId2 = option2,
            Stock = variant.Stock.Value
        };

        _ = dimensions;
        return Result<LegacyGradeVariantRow>.Success(row);
    }
}
