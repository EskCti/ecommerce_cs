namespace RetailOps.Core.Catalog.Application.DTOs;

public enum GradeConfigurationAction
{
    AddDimension,
    AddOption,
    RemoveGrade,
    AdjustGradeStock
}

public sealed record ConfigureProductGradeInputDto
{
    public required GradeConfigurationAction Action { get; init; }
    public string? DimensionName { get; init; }
    public Guid? DimensionId { get; init; }
    public Guid? OptionId { get; init; }
    public string? OptionLabel { get; init; }
    public int? Stock { get; init; }
}

public sealed record GradeDimensionOutputDto
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<GradeOptionOutputDto> Options { get; init; }

    public static GradeDimensionOutputDto FromDomain(Domain.Entities.GradeDimension dimension)
    {
        return new GradeDimensionOutputDto
        {
            Id = dimension.Id,
            ProductId = dimension.ProductId,
            Name = dimension.Name.Value,
            Options = dimension.Options.Select(GradeOptionOutputDto.FromDomain).ToList()
        };
    }
}

public sealed record GradeOptionOutputDto
{
    public required Guid Id { get; init; }
    public required Guid DimensionId { get; init; }
    public required string Label { get; init; }
    public required int Stock { get; init; }

    public static GradeOptionOutputDto FromDomain(Domain.Entities.GradeOption option)
    {
        return new GradeOptionOutputDto
        {
            Id = option.Id,
            DimensionId = option.DimensionId,
            Label = option.Label.Value,
            Stock = option.Stock.Value
        };
    }
}
