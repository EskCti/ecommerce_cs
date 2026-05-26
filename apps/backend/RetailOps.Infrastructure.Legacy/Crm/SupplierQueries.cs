using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Application.Queries;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Crm;

public sealed class SupplierQueries(ISupplierRepository repository) : ISupplierQueries
{
    public async Task<Result<IReadOnlyList<SupplierListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        SupplierListFilter? filter = null,
        CancellationToken ct = default)
    {
        var result = await repository.GetByTenantId(tenantId);
        if (result.IsFailure)
            return Result<IReadOnlyList<SupplierListItemDto>>.Failure(result.Error);

        var items = result.Value.AsEnumerable();

        if (filter?.IsActive is bool isActive)
            items = items.Where(s => s.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(filter?.Name))
        {
            var term = filter.Name.Trim();
            items = items.Where(s => s.Name.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (filter?.PersonType is not null)
            items = items.Where(s => s.PersonType == filter.PersonType);

        if (!string.IsNullOrWhiteSpace(filter?.TaxDocument))
        {
            var docTerm = new string(filter.TaxDocument.Where(char.IsDigit).ToArray());
            items = items.Where(s => s.TaxDocument.Value.Contains(docTerm, StringComparison.Ordinal));
        }

        var page = Math.Max(1, filter?.Page ?? 1);
        var pageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 100);

        var projected = items
            .OrderBy(s => s.Name.Value)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(SupplierListItemDto.FromDomain)
            .ToList();

        return Result<IReadOnlyList<SupplierListItemDto>>.Success(projected);
    }
}
