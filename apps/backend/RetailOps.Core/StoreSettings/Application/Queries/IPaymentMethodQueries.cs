using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.Queries;

public interface IPaymentMethodQueries
{
    Task<Result<IReadOnlyList<PaymentMethodListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        PaymentMethodListFilter? filter = null,
        CancellationToken ct = default);
    
    Task<Result<PaymentMethodListItemDto?>> GetByIdAsync(
        TenantId tenantId,
        Guid paymentMethodId,
        CancellationToken ct = default);
}

public sealed record PaymentMethodListFilter(
    bool? IsActive,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20);