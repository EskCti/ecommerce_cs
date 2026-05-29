using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.Services;

public sealed class GradeVariantSyncService
{
    public const int MaxDimensions = 2;
    public const int MaxOptionsPerDimension = 30;

    public Result SyncAfterAddOption(Product product, GradeOption addedOption, int? initialStock = null)
    {
        var dimensionCount = product.GradeDimensions.Count;
        if (dimensionCount > MaxDimensions)
            return Result.Failure($"A product may have at most {MaxDimensions} grade dimensions.");

        // Combinações (variantes) são cadastradas manualmente via AddVariant.
        _ = initialStock;
        _ = addedOption;
        return Result.Success();
    }

    public Result SyncAfterRemoveOption(Product product, Guid optionId)
    {
        product.RemoveVariantsReferencingOption(optionId);
        return Result.Success();
    }

    public Result SyncAfterRemoveDimension(Product product, Guid dimensionId)
    {
        var dimension = product.GradeDimensions.FirstOrDefault(d => d.Id == dimensionId);
        if (dimension is null)
            return Result.Success();

        foreach (var option in dimension.Options)
            product.RemoveVariantsReferencingOption(option.Id);

        return Result.Success();
    }

    public Result SyncAfterAddDimension(Product product)
    {
        if (product.GradeDimensions.Count != 2)
            return Result.Success();

        foreach (var dimension in product.GradeDimensions)
        {
            foreach (var option in dimension.Options)
                option.ClearStockForTwoDimensions();
        }

        return Result.Success();
    }

    public Result RebuildCartesianVariants(Product product, bool preserveSingleDimensionStock = false)
    {
        product.ClearGradeVariants();

        if (product.GradeDimensions.Count == 0)
            return Result.Success();

        if (product.GradeDimensions.Count == 1)
        {
            var dimension = product.GradeDimensions[0];
            foreach (var option in dimension.Options)
            {
                var stock = preserveSingleDimensionStock
                    ? option.Stock
                    : StockQuantity.Create(0).Value;

                var add = product.AddGradeVariant([option.Id], stock);
                if (add.IsFailure)
                    return add;
            }

            return Result.Success();
        }

        if (product.GradeDimensions.Count == 2)
        {
            var dim1 = product.GradeDimensions[0];
            var dim2 = product.GradeDimensions[1];

            foreach (var option1 in dim1.Options)
            {
                foreach (var option2 in dim2.Options)
                {
                    var add = product.AddGradeVariant(
                        [option1.Id, option2.Id],
                        StockQuantity.Create(0).Value);

                    if (add.IsFailure)
                        return add;
                }
            }
        }

        return Result.Success();
    }

    public Result MigrateOptionsStockToVariants(Product product)
    {
        if (product.GradeVariants.Count > 0)
            return Result.Success();

        return RebuildCartesianVariants(product, preserveSingleDimensionStock: true);
    }

    private static IReadOnlyList<Guid> OrderOptionIds(Product product, Guid optionIdA, Guid optionIdB)
    {
        var dimIndexA = FindDimensionIndex(product, optionIdA);
        var dimIndexB = FindDimensionIndex(product, optionIdB);
        return dimIndexA <= dimIndexB
            ? [optionIdA, optionIdB]
            : [optionIdB, optionIdA];
    }

    private static int FindDimensionIndex(Product product, Guid optionId)
    {
        for (var i = 0; i < product.GradeDimensions.Count; i++)
        {
            if (product.GradeDimensions[i].Options.Any(o => o.Id == optionId))
                return i;
        }

        return int.MaxValue;
    }
}
