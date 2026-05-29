namespace RetailOps.Core.Catalog.Application.DTOs;

public enum GradeConfigurationAction
{
    AddDimension,
    AddOption,
    RemoveGrade,
    AdjustGradeStock,
    AddVariant,
    AdjustVariantStock,
    RemoveVariant
}

public sealed record ConfigureProductGradeInputDto
{
    public required GradeConfigurationAction Action { get; init; }
    public string? DimensionName { get; init; }
    public Guid? DimensionId { get; init; }
    public Guid? OptionId { get; init; }
    public string? OptionLabel { get; init; }
    public int? Stock { get; init; }
    public Guid? VariantId { get; init; }
    public IReadOnlyList<Guid>? OptionIds { get; init; }
}

public sealed record GradeVariantOutputDto
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required IReadOnlyList<Guid> OptionIds { get; init; }
    public required string Label { get; init; }
    public required int Stock { get; init; }

    public static GradeVariantOutputDto FromDomain(
        Domain.Entities.GradeVariant variant,
        Domain.Entities.Product product)
    {
        return new GradeVariantOutputDto
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            OptionIds = variant.OptionIds.ToList(),
            Label = variant.BuildLabel(product.GradeDimensions),
            Stock = variant.Stock.Value
        };
    }
}

public sealed record GradeConfigurationOutputDto
{
    public required IReadOnlyList<GradeDimensionOutputDto> Dimensions { get; init; }
    public required IReadOnlyList<GradeVariantOutputDto> Variants { get; init; }

    public static GradeConfigurationOutputDto FromDomain(Domain.Entities.Product product)
    {
        var hideOptionStock = product.HasTwoGradeDimensions();
        return new GradeConfigurationOutputDto
        {
            Dimensions = product.GradeDimensions
                .Select(d => GradeDimensionOutputDto.FromDomain(d, hideOptionStock))
                .ToList(),
            Variants = product.GradeVariants
                .Select(v => GradeVariantOutputDto.FromDomain(v, product))
                .ToList()
        };
    }

    public static GradeConfigurationOutputDto FromProductOutput(ProductOutputDto product) =>
        new()
        {
            Dimensions = product.GradeDimensions,
            Variants = product.GradeVariants
        };
}

public sealed record GradeDimensionOutputDto
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<GradeOptionOutputDto> Options { get; init; }

    public static GradeDimensionOutputDto FromDomain(
        Domain.Entities.GradeDimension dimension,
        bool hideOptionStock = false)
    {
        return new GradeDimensionOutputDto
        {
            Id = dimension.Id,
            ProductId = dimension.ProductId,
            Name = dimension.Name.Value,
            Options = dimension.Options
                .Select(o => GradeOptionOutputDto.FromDomain(o, hideOptionStock))
                .ToList()
        };
    }
}

public sealed record GradeOptionOutputDto
{
    public required Guid Id { get; init; }
    public required Guid DimensionId { get; init; }
    public required string Label { get; init; }
    public required int Stock { get; init; }

    public static GradeOptionOutputDto FromDomain(
        Domain.Entities.GradeOption option,
        bool hideOptionStock = false)
    {
        return new GradeOptionOutputDto
        {
            Id = option.Id,
            DimensionId = option.DimensionId,
            Label = option.Label.Value,
            Stock = hideOptionStock ? 0 : option.Stock.Value
        };
    }
}
