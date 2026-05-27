namespace RetailOps.Core.Catalog.Application.DTOs;

public sealed record CreateCategoryInputDto
{
    public required string Name { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed record UpdateCategoryInputDto
{
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
}

public sealed record CategoryOutputDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static CategoryOutputDto FromDomain(Domain.Entities.Category category)
    {
        return new CategoryOutputDto
        {
            Id = category.Id,
            TenantId = category.TenantId.Value,
            Name = category.Name.Value,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}

public sealed record CategoryListItemDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }

    public static CategoryListItemDto FromDomain(Domain.Entities.Category category)
    {
        return new CategoryListItemDto
        {
            Id = category.Id,
            Name = category.Name.Value,
            IsActive = category.IsActive
        };
    }
}
