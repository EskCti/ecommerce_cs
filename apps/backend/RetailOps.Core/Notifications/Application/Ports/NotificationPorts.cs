using RetailOps.Core.Notifications.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Notifications.Application.Ports;

public sealed record ReceivablesDueTodaySummary(int Count, decimal TotalAmount);

public interface IReceivablesDueTodayQueryPort
{
    Task<Result<ReceivablesDueTodaySummary>> GetDueTodaySummaryAsync(
        TenantId tenantId,
        CancellationToken ct = default);
}

public sealed record LowStockSummary(int Count, IReadOnlyList<string> TopProductNames);

public interface ILowStockSummaryQueryPort
{
    Task<Result<LowStockSummary>> GetLowStockSummaryAsync(
        TenantId tenantId,
        CancellationToken ct = default);
}

public interface ITenantBillingAlertQueryPort
{
    Task<Result<string?>> GetBillingAlertAsync(
        TenantId tenantId,
        CancellationToken ct = default);
}

public interface IActiveTenantIdsQueryPort
{
    Task<Result<IReadOnlyList<TenantId>>> ListActiveTenantIdsAsync(CancellationToken ct = default);
}

public interface IWhatsAppGatewayPort
{
    Task<Result> SendTextAsync(
        PhoneNumber to,
        MessageTemplate body,
        WhatsAppCredentials credentials,
        CancellationToken ct = default);
}

public interface IWhatsAppCredentialsResolver
{
    Task<Result<WhatsAppCredentials>> ResolveAsync(TenantId tenantId, CancellationToken ct = default);
}

public interface INotificationStoreNamePort
{
    Task<Result<string>> GetStoreNameAsync(TenantId tenantId, CancellationToken ct = default);
}
