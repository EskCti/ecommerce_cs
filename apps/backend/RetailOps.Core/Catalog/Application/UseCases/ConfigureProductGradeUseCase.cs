using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed class ConfigureProductGradeUseCase(
    IProductRepository productRepository,
    StockAdjustmentPolicy stockAdjustmentPolicy,
    GradeVariantSyncService gradeVariantSync)
    : IUseCase<(int tenantId, Guid productId, ConfigureProductGradeInputDto input), ProductOutputDto>
{
    public async Task<Result<ProductOutputDto>> Execute(
        (int tenantId, Guid productId, ConfigureProductGradeInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, productId, input) = request;

        var productResult = await productRepository.GetById(productId);
        if (productResult.IsFailure)
            return Result<ProductOutputDto>.Failure(productResult.Error);

        var product = productResult.Value;
        if (product.TenantId.Value != tenantId)
            return Result<ProductOutputDto>.Failure("Product does not belong to this tenant.");

        var actionResult = input.Action switch
        {
            GradeConfigurationAction.AddDimension => AddDimension(product, input),
            GradeConfigurationAction.AddOption => AddOption(product, input),
            GradeConfigurationAction.RemoveGrade => RemoveGrade(product, input),
            GradeConfigurationAction.AdjustGradeStock => AdjustGradeStock(product, input),
            GradeConfigurationAction.AddVariant => AddVariant(product, input),
            GradeConfigurationAction.AdjustVariantStock => AdjustVariantStock(product, input),
            GradeConfigurationAction.RemoveVariant => RemoveVariant(product, input),
            _ => Result.Failure("Unknown grade configuration action.")
        };

        if (actionResult.IsFailure)
            return Result<ProductOutputDto>.Failure(actionResult.Error);

        var syncStock = product.SyncAggregateStockFromGrades();
        if (syncStock.IsFailure)
            return Result<ProductOutputDto>.Failure(syncStock.Error);

        var saveResult = await productRepository.Save(product);
        if (saveResult.IsFailure)
            return Result<ProductOutputDto>.Failure(saveResult.Error);

        return Result<ProductOutputDto>.Success(ProductOutputDto.FromDomain(product));
    }

    private Result AddDimension(Product product, ConfigureProductGradeInputDto input)
    {
        if (string.IsNullOrWhiteSpace(input.DimensionName))
            return Result.Failure("Dimension name is required.");

        var nameResult = ProductName.Create(input.DimensionName);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        var dimensionResult = GradeDimension.Create(product.Id, nameResult.Value);
        if (dimensionResult.IsFailure)
            return Result.Failure(dimensionResult.Error);

        var addResult = product.AddGradeDimension(dimensionResult.Value);
        if (addResult.IsFailure)
            return addResult;

        if (product.GradeDimensions.Count == 2)
        {
            var sync = gradeVariantSync.SyncAfterAddDimension(product);
            if (sync.IsFailure)
                return sync;
        }

        return Result.Success();
    }

    private Result AddOption(Product product, ConfigureProductGradeInputDto input)
    {
        if (input.DimensionId is null)
            return Result.Failure("Dimension id is required.");

        if (string.IsNullOrWhiteSpace(input.OptionLabel))
            return Result.Failure("Option label is required.");

        var dimension = product.GradeDimensions.FirstOrDefault(d => d.Id == input.DimensionId);
        if (dimension is null)
            return Result.Failure("Grade dimension not found.");

        if (dimension.Options.Count >= GradeVariantSyncService.MaxOptionsPerDimension)
            return Result.Failure($"Each dimension may have at most {GradeVariantSyncService.MaxOptionsPerDimension} options.");

        if (product.HasTwoGradeDimensions() && input.Stock is > 0)
            return Result.Failure("Stock must be set on grade variants when the product has two dimensions.");

        var labelResult = ProductName.Create(input.OptionLabel);
        if (labelResult.IsFailure)
            return Result.Failure(labelResult.Error);

        var stockValue = product.HasTwoGradeDimensions() ? 0 : input.Stock ?? 0;
        var stockResult = StockQuantity.Create(stockValue);
        if (stockResult.IsFailure)
            return Result.Failure(stockResult.Error);

        var optionResult = dimension.AddOption(labelResult.Value, stockResult.Value);
        if (optionResult.IsFailure)
            return Result.Failure(optionResult.Error);

        return gradeVariantSync.SyncAfterAddOption(product, optionResult.Value, input.Stock);
    }

    private Result RemoveGrade(Product product, ConfigureProductGradeInputDto input)
    {
        if (input.OptionId.HasValue)
        {
            var optionId = input.OptionId.Value;
            if (product.GradeVariants.Any(v => v.ReferencesOption(optionId)))
                return Result.Failure("Cannot remove option while combinations use it. Delete those combinations first.");

            foreach (var dimension in product.GradeDimensions)
            {
                var removeResult = dimension.RemoveOption(optionId);
                if (removeResult.IsSuccess)
                {
                    gradeVariantSync.SyncAfterRemoveOption(product, optionId);
                    return Result.Success();
                }
            }

            return Result.Failure("Grade option not found.");
        }

        if (input.DimensionId is null)
            return Result.Failure("Dimension id or option id is required.");

        gradeVariantSync.SyncAfterRemoveDimension(product, input.DimensionId.Value);
        var removeDimension = product.RemoveGradeDimension(input.DimensionId.Value);
        if (removeDimension.IsFailure)
            return removeDimension;

        return gradeVariantSync.RebuildCartesianVariants(product, preserveSingleDimensionStock: true);
    }

    private Result AdjustGradeStock(Product product, ConfigureProductGradeInputDto input)
    {
        if (product.HasTwoGradeDimensions())
            return Result.Failure("Use AdjustVariantStock when the product has two grade dimensions.");

        if (input.OptionId is null || !input.Stock.HasValue)
            return Result.Failure("Option id and stock are required.");

        var stockResult = StockQuantity.Create(input.Stock.Value);
        if (stockResult.IsFailure)
            return Result.Failure(stockResult.Error);

        foreach (var dimension in product.GradeDimensions)
        {
            var option = dimension.Options.FirstOrDefault(o => o.Id == input.OptionId);
            if (option is null)
                continue;

            var validation = stockAdjustmentPolicy.ValidateAdjustment(
                option.Stock,
                stockResult.Value,
                isExit: stockResult.Value.Value < option.Stock.Value);

            if (validation.IsFailure)
                return validation;

            var adjustOption = option.AdjustStock(stockResult.Value);
            if (adjustOption.IsFailure)
                return adjustOption;

            var variant = product.GradeVariants.FirstOrDefault(v => v.MatchesOptions([option.Id]));
            if (variant is null)
                return product.AddGradeVariant([option.Id], stockResult.Value).IsFailure
                    ? Result.Failure("Failed to create grade variant.")
                    : Result.Success();

            return product.AdjustVariantStock(variant.Id, stockResult.Value);
        }

        return Result.Failure("Grade option not found.");
    }

    private Result AddVariant(Product product, ConfigureProductGradeInputDto input)
    {
        if (input.OptionIds is null || input.OptionIds.Count is < 1 or > 2)
            return Result.Failure("One or two option ids are required.");

        if (input.OptionIds.Count != product.GradeDimensions.Count)
            return Result.Failure("Select one option per dimension.");

        var orderedOptionIds = OrderOptionIdsByDimension(product, input.OptionIds);
        if (orderedOptionIds.Count != input.OptionIds.Count)
            return Result.Failure("Invalid option ids for this product.");

        var stockResult = StockQuantity.Create(input.Stock ?? 0);
        if (stockResult.IsFailure)
            return Result.Failure(stockResult.Error);

        var add = product.AddGradeVariant(orderedOptionIds, stockResult.Value);
        return add.IsFailure ? Result.Failure(add.Error) : Result.Success();
    }

    private static List<Guid> OrderOptionIdsByDimension(Product product, IReadOnlyList<Guid> optionIds)
    {
        var ordered = new List<Guid>();
        foreach (var dimension in product.GradeDimensions)
        {
            var match = dimension.Options.FirstOrDefault(o => optionIds.Contains(o.Id));
            if (match is not null)
                ordered.Add(match.Id);
        }

        return ordered;
    }

    private Result AdjustVariantStock(Product product, ConfigureProductGradeInputDto input)
    {
        if (input.VariantId is null || !input.Stock.HasValue)
            return Result.Failure("Variant id and stock are required.");

        var variantResult = product.FindVariantById(input.VariantId.Value);
        if (variantResult.IsFailure || variantResult.Value is null)
            return Result.Failure("Grade variant not found.");

        var stockResult = StockQuantity.Create(input.Stock.Value);
        if (stockResult.IsFailure)
            return Result.Failure(stockResult.Error);

        var variant = variantResult.Value;
        var validation = stockAdjustmentPolicy.ValidateAdjustment(
            variant.Stock,
            stockResult.Value,
            isExit: stockResult.Value.Value < variant.Stock.Value);

        if (validation.IsFailure)
            return validation;

        return product.AdjustVariantStock(variant.Id, stockResult.Value);
    }

    private Result RemoveVariant(Product product, ConfigureProductGradeInputDto input)
    {
        if (input.VariantId is null)
            return Result.Failure("Variant id is required.");

        return product.RemoveGradeVariant(input.VariantId.Value);
    }
}
