namespace RetailOps.Core.Catalog.Application.DTOs;

public sealed record CreateProductInputDto
{
    public required string Barcode { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal SalePrice { get; init; }
    public required decimal CostPrice { get; init; }
    public int InitialStock { get; init; }
    public int StockAlertLevel { get; init; }
    public required Guid CategoryId { get; init; }
    public Guid? SupplierId { get; init; }
    public string? PhotoPath { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed record UpdateProductInputDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? SalePrice { get; init; }
    public decimal? CostPrice { get; init; }
    public int? StockAlertLevel { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? SupplierId { get; init; }
    public string? PhotoPath { get; init; }
    public bool? IsActive { get; init; }
    public bool ClearSupplier { get; init; }
}

public sealed record ProductOutputDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required string Barcode { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal SalePrice { get; init; }
    public required decimal CostPrice { get; init; }
    public required int Stock { get; init; }
    public required decimal ProfitMargin { get; init; }
    public required int StockAlertLevel { get; init; }
    public required Guid CategoryId { get; init; }
    public Guid? SupplierId { get; init; }
    public string? PhotoPath { get; init; }
    public required bool IsActive { get; init; }
    public required bool IsOpenPrice { get; init; }
    public required bool IsLowStock { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required IReadOnlyList<GradeDimensionOutputDto> GradeDimensions { get; init; }
    public required IReadOnlyList<GradeVariantOutputDto> GradeVariants { get; init; }

    public static ProductOutputDto FromDomain(Domain.Entities.Product product)
    {
        return new ProductOutputDto
        {
            Id = product.Id,
            TenantId = product.TenantId.Value,
            Barcode = product.Barcode.Value,
            Name = product.Name.Value,
            Description = product.Description,
            SalePrice = product.SalePrice.Value,
            CostPrice = product.CostPrice.Value,
            Stock = product.Stock.Value,
            ProfitMargin = product.ProfitMargin.Value,
            StockAlertLevel = product.StockAlertLevel.Value,
            CategoryId = product.CategoryId.Value,
            SupplierId = product.SupplierId,
            PhotoPath = product.PhotoPath?.Value,
            IsActive = product.IsActive,
            IsOpenPrice = product.IsOpenPrice,
            IsLowStock = product.IsLowStock(),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            GradeDimensions = product.GradeDimensions
                .Select(d => GradeDimensionOutputDto.FromDomain(d, product.HasTwoGradeDimensions()))
                .ToList(),
            GradeVariants = product.GradeVariants
                .Select(v => GradeVariantOutputDto.FromDomain(v, product))
                .ToList()
        };
    }
}

public sealed record ProductListItemDto
{
    public required Guid Id { get; init; }
    public required string Barcode { get; init; }
    public required string Name { get; init; }
    public required decimal SalePrice { get; init; }
    public required int Stock { get; init; }
    public required Guid CategoryId { get; init; }
    public required bool IsActive { get; init; }
    public required bool IsLowStock { get; init; }

    public static ProductListItemDto FromDomain(Domain.Entities.Product product)
    {
        return new ProductListItemDto
        {
            Id = product.Id,
            Barcode = product.Barcode.Value,
            Name = product.Name.Value,
            SalePrice = product.SalePrice.Value,
            Stock = product.Stock.Value,
            CategoryId = product.CategoryId.Value,
            IsActive = product.IsActive,
            IsLowStock = product.IsLowStock()
        };
    }
}

public sealed record GenerateBarcodeOutputDto
{
    public required string Barcode { get; init; }
}

public sealed record PaginatedProductsOutputDto
{
    public required IReadOnlyList<ProductListItemDto> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
}
