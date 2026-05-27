namespace RetailOps.Core.Catalog.Application.Events;

public sealed record ProductPurchased(
    Guid ProductId,
    int TenantId,
    int Quantity,
    decimal UnitCost,
    DateTime PurchasedAt);

public interface IProductPurchasedPublisher
{
    Task PublishAsync(ProductPurchased @event, CancellationToken ct = default);
}
