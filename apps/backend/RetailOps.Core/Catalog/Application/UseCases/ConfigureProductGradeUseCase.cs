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
    StockAdjustmentPolicy stockAdjustmentPolicy)
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
            _ => Result.Failure("Unknown grade configuration action.")
        };

        if (actionResult.IsFailure)
            return Result<ProductOutputDto>.Failure(actionResult.Error);

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

        return product.AddGradeDimension(dimensionResult.Value);
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

        var labelResult = ProductName.Create(input.OptionLabel);
        if (labelResult.IsFailure)
            return Result.Failure(labelResult.Error);

        var stockResult = StockQuantity.Create(input.Stock ?? 0);
        if (stockResult.IsFailure)
            return Result.Failure(stockResult.Error);

        var optionResult = dimension.AddOption(labelResult.Value, stockResult.Value);
        return optionResult.IsFailure ? Result.Failure(optionResult.Error) : Result.Success();
    }

    private Result RemoveGrade(Product product, ConfigureProductGradeInputDto input)
    {
        if (input.OptionId.HasValue)
        {
            foreach (var dimension in product.GradeDimensions)
            {
                var removeResult = dimension.RemoveOption(input.OptionId.Value);
                if (removeResult.IsSuccess)
                    return Result.Success();
            }

            return Result.Failure("Grade option not found.");
        }

        if (input.DimensionId is null)
            return Result.Failure("Dimension id or option id is required.");

        return product.RemoveGradeDimension(input.DimensionId.Value);
    }

    private Result AdjustGradeStock(Product product, ConfigureProductGradeInputDto input)
    {
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

            var currentStock = option.Stock;
            var validation = stockAdjustmentPolicy.ValidateAdjustment(
                currentStock,
                stockResult.Value,
                isExit: stockResult.Value.Value < currentStock.Value);

            if (validation.IsFailure)
                return validation;

            return option.AdjustStock(stockResult.Value);
        }

        return Result.Failure("Grade option not found.");
    }
}
