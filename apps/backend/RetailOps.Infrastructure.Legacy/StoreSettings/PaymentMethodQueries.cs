using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Application.Queries;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.StoreSettings;

public sealed class PaymentMethodQueries(IPaymentMethodRepository repository) : IPaymentMethodQueries
{
    public async Task<Result<IReadOnlyList<PaymentMethodListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        PaymentMethodListFilter? filter = null,
        CancellationToken ct = default)
    {
        var result = await repository.GetByTenantId(tenantId);
        if (result.IsFailure)
            return Result<IReadOnlyList<PaymentMethodListItemDto>>.Failure(result.Error);

        var items = result.Value.AsEnumerable();

        if (filter?.IsActive is bool isActive)
            items = items.Where(p => p.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(filter?.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            items = items.Where(p => p.Name.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var page = Math.Max(1, filter?.Page ?? 1);
        var pageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 100);

        var projected = items
            .OrderBy(p => p.Name.Value)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(PaymentMethodListItemDto.FromDomain)
            .ToList();

        return Result<IReadOnlyList<PaymentMethodListItemDto>>.Success(projected);
    }

    public async Task<Result<PaymentMethodListItemDto?>> GetByIdAsync(
        TenantId tenantId,
        Guid paymentMethodId,
        CancellationToken ct = default)
    {
        var result = await repository.GetById(paymentMethodId);
        if (result.IsFailure)
            return Result<PaymentMethodListItemDto?>.Failure(result.Error);

        if (result.Value.TenantId.Value != tenantId.Value)
            return Result<PaymentMethodListItemDto?>.Success(null);

        return Result<PaymentMethodListItemDto?>.Success(
            PaymentMethodListItemDto.FromDomain(result.Value));
    }
}
