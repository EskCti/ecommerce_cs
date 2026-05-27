using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Core.Catalog.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Domain.Entities;

public sealed class GradeMovementDetail : Entity
{
    public MovementType Type { get; private set; }
    public int MovementId { get; private set; }
    public Guid GradeOptionId { get; private set; }
    public StockQuantity Quantity { get; private set; } = null!;

    private GradeMovementDetail() { }

    public static GradeMovementDetail Create(
        MovementType type,
        int movementId,
        Guid gradeOptionId,
        StockQuantity quantity) =>
        new()
        {
            Type = type,
            MovementId = movementId,
            GradeOptionId = gradeOptionId,
            Quantity = quantity
        };
}
