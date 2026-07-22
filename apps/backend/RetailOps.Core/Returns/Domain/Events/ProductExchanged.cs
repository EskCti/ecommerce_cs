namespace RetailOps.Core.Returns.Domain.Events;

public sealed record ProductExchanged(
    Guid ExchangeId,
    int TenantId,
    Guid CustomerId,
    Guid ProductInId,
    Guid ProductOutId,
    DateTime OccurredAt);
