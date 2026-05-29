using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Infrastructure.Legacy.Mappers.Catalog;

public static class LegacyGradeDetailMapper
{
    public static Result<GradeDimension> ToDimensionDomain(
        LegacyGradeDimensionRow row,
        IEnumerable<GradeOption>? options = null)
    {
        var nameResult = ProductName.Create(row.Name);
        if (nameResult.IsFailure)
            return Result<GradeDimension>.Failure(nameResult.Error);

        var productId = LegacyCatalogIds.Product(row.ProductLegacyId);

        return GradeDimension.Reconstitute(
            LegacyCatalogIds.GradeDimension(row.Id),
            productId,
            nameResult.Value,
            options);
    }

    public static Result<GradeOption> ToOptionDomain(LegacyGradeOptionRow row)
    {
        var labelResult = ProductName.Create(row.Label);
        if (labelResult.IsFailure)
            return Result<GradeOption>.Failure(labelResult.Error);

        var stockResult = StockQuantity.Create(row.Stock);
        if (stockResult.IsFailure)
            return Result<GradeOption>.Failure(stockResult.Error);

        var dimensionId = LegacyCatalogIds.GradeDimension(row.DimensionLegacyId);

        return GradeOption.Reconstitute(
            LegacyCatalogIds.GradeOption(row.Id),
            dimensionId,
            labelResult.Value,
            stockResult.Value);
    }

    public static Result<LegacyGradeDimensionRow> ToDimensionLegacy(
        GradeDimension dimension,
        int productLegacyId,
        int? legacyId = null)
    {
        var row = new LegacyGradeDimensionRow
        {
            Id = legacyId ?? LegacyCatalogIds.ParseLegacyId(dimension.Id, "0011") ?? 0,
            ProductLegacyId = productLegacyId,
            Name = dimension.Name.Value
        };

        return Result<LegacyGradeDimensionRow>.Success(row);
    }

    public static Result<LegacyGradeOptionRow> ToOptionLegacy(
        GradeOption option,
        int dimensionLegacyId,
        int? legacyId = null)
    {
        var row = new LegacyGradeOptionRow
        {
            Id = legacyId ?? LegacyCatalogIds.ParseLegacyId(option.Id, "0012") ?? 0,
            DimensionLegacyId = dimensionLegacyId,
            Label = option.Label.Value,
            Stock = option.Stock.Value
        };

        return Result<LegacyGradeOptionRow>.Success(row);
    }

    public static Result<LegacyGradeMovementDetailRow> ToMovementDetailLegacy(
        GradeMovementDetail detail,
        int movementLegacyId,
        int optionLegacyId,
        int? optionLegacyId2 = null,
        int? legacyId = null)
    {
        var row = new LegacyGradeMovementDetailRow
        {
            Id = legacyId ?? 0,
            MovementType = detail.Type.ToString(),
            MovementLegacyId = movementLegacyId,
            OptionLegacyId = optionLegacyId,
            OptionLegacyId2 = optionLegacyId2
                ?? (detail.GradeOptionId2 is Guid second
                    ? LegacyCatalogIds.ParseLegacyId(second, "0012")
                    : null),
            Quantity = detail.Quantity.Value
        };

        return Result<LegacyGradeMovementDetailRow>.Success(row);
    }
}
