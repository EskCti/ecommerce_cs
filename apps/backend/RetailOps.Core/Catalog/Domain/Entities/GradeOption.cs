using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Core.Catalog.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Domain.Entities;

public sealed class GradeOption : Entity
{
    public Guid DimensionId { get; private set; }
    public ProductName Label { get; private set; } = null!;
    public StockQuantity Stock { get; private set; } = null!;

    private GradeOption() { }

    private GradeOption(Guid dimensionId, ProductName label, StockQuantity stock)
    {
        DimensionId = dimensionId;
        Label = label;
        Stock = stock;
    }

    public static Result<GradeOption> Create(Guid dimensionId, ProductName label, StockQuantity stock) =>
        Result<GradeOption>.Success(new GradeOption(dimensionId, label, stock));

    public static Result<GradeOption> Reconstitute(
        Guid id,
        Guid dimensionId,
        ProductName label,
        StockQuantity stock)
    {
        var option = new GradeOption(dimensionId, label, stock) { Id = id };
        return Result<GradeOption>.Success(option);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public Result AdjustStock(StockQuantity newStock)
    {
        Stock = newStock;
        return Result.Success();
    }

    internal void ClearStockForTwoDimensions()
    {
        Stock = StockQuantity.Create(0).Value;
    }
}
