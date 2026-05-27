using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Core.Catalog.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Domain.Entities;

public sealed class StockMovement : Entity
{
    public Guid ProductId { get; private set; }
    public StockQuantity Quantity { get; private set; } = null!;
    public string Reason { get; private set; } = string.Empty;
    public int UserId { get; private set; }
    public StockMovementType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StockMovement() { }

    private StockMovement(
        Guid productId,
        StockQuantity quantity,
        string reason,
        int userId,
        StockMovementType type)
    {
        ProductId = productId;
        Quantity = quantity;
        Reason = reason;
        UserId = userId;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<StockMovement> Create(
        Guid productId,
        StockQuantity quantity,
        string reason,
        int userId,
        StockMovementType type)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result<StockMovement>.Failure("Stock movement reason is required.");

        return Result<StockMovement>.Success(new StockMovement(
            productId,
            quantity,
            reason.Trim(),
            userId,
            type));
    }

    public static Result<StockMovement> Reconstitute(
        Guid id,
        Guid productId,
        StockQuantity quantity,
        string reason,
        int userId,
        StockMovementType type,
        DateTime createdAt)
    {
        var movement = new StockMovement(productId, quantity, reason, userId, type)
        {
            Id = id,
            CreatedAt = createdAt
        };

        return Result<StockMovement>.Success(movement);
    }

    internal void SyncIdentity(Guid id) => Id = id;
}
