namespace RetailOps.Core.Catalog.Application.DTOs;

public sealed record RecordStockMovementInputDto
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required string Reason { get; init; }
    public required int UserId { get; init; }
}

public sealed record PurchaseStockInputDto
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitCost { get; init; }
    public required string Reason { get; init; }
    public required int UserId { get; init; }
    public DateTime? DueDate { get; init; }
    public int? SupplierLegacyId { get; init; }
}

public sealed record StockMovementOutputDto
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required string Reason { get; init; }
    public required int UserId { get; init; }
    public required string Type { get; init; }
    public required DateTime CreatedAt { get; init; }

    public static StockMovementOutputDto FromDomain(Domain.Entities.StockMovement movement)
    {
        return new StockMovementOutputDto
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            Quantity = movement.Quantity.Value,
            Reason = movement.Reason,
            UserId = movement.UserId,
            Type = movement.Type.ToString(),
            CreatedAt = movement.CreatedAt
        };
    }
}

public sealed record LowStockProductDto
{
    public required Guid Id { get; init; }
    public required string Barcode { get; init; }
    public required string Name { get; init; }
    public required int Stock { get; init; }
    public required int StockAlertLevel { get; init; }

    public static LowStockProductDto FromDomain(Domain.Entities.Product product)
    {
        return new LowStockProductDto
        {
            Id = product.Id,
            Barcode = product.Barcode.Value,
            Name = product.Name.Value,
            Stock = product.Stock.Value,
            StockAlertLevel = product.StockAlertLevel.Value
        };
    }
}
