using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.Ports;

public interface IStoreSettingsLegacyPort
{
    // StoreConfig operations
    Task<Result<StoreConfig?>> GetStoreConfigFromLegacyAsync(TenantId tenantId, CancellationToken ct = default);
    Task<Result> SaveStoreConfigToLegacyAsync(StoreConfig storeConfig, CancellationToken ct = default);
    
    // PaymentMethod operations
    Task<Result<IReadOnlyList<PaymentMethod>>> GetPaymentMethodsFromLegacyAsync(TenantId tenantId, CancellationToken ct = default);
    Task<Result<PaymentMethod?>> GetPaymentMethodFromLegacyAsync(TenantId tenantId, Guid paymentMethodId, CancellationToken ct = default);
    Task<Result<int>> SavePaymentMethodToLegacyAsync(PaymentMethod paymentMethod, CancellationToken ct = default);
    Task<Result> DeletePaymentMethodFromLegacyAsync(TenantId tenantId, Guid paymentMethodId, CancellationToken ct = default);
    
    // CashRegisterTerminal operations
    Task<Result<IReadOnlyList<CashRegisterTerminal>>> GetCashRegisterTerminalsFromLegacyAsync(TenantId tenantId, CancellationToken ct = default);
    Task<Result<CashRegisterTerminal?>> GetCashRegisterTerminalFromLegacyAsync(TenantId tenantId, Guid terminalId, CancellationToken ct = default);
    Task<Result<int>> SaveCashRegisterTerminalToLegacyAsync(CashRegisterTerminal terminal, CancellationToken ct = default);
    Task<Result> DeleteCashRegisterTerminalFromLegacyAsync(TenantId tenantId, Guid terminalId, CancellationToken ct = default);
}