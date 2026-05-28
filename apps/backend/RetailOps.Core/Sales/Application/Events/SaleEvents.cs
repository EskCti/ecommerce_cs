namespace RetailOps.Core.Sales.Application.Events;

public sealed record CashSessionOpenedEvent(
    Guid SessionId,
    int TenantId,
    Guid TerminalId,
    Guid OperatorUserId,
    decimal OpeningFloat,
    DateTime OpenedAt);

public sealed record SaleCompletedEvent(
    Guid SaleId,
    int TenantId,
    Guid CashSessionId,
    Guid OperatorUserId,
    decimal Total,
    decimal CommissionAmount,
    DateTime CompletedAt);

public sealed record SaleCancelledEvent(
    Guid SaleId,
    int TenantId,
    DateTime CancelledAt);
