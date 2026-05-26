using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Application.Ports;
using RetailOps.Core.Crm.Application.Queries;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Crm;

public sealed class CustomerQueries(
    ICustomerRepository repository,
    ICrmLegacyPort legacyPort) : ICustomerQueries
{
    public async Task<Result<IReadOnlyList<CustomerListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        CustomerListFilter? filter = null,
        CancellationToken ct = default)
    {
        var result = await repository.GetByTenantId(tenantId);
        if (result.IsFailure)
            return Result<IReadOnlyList<CustomerListItemDto>>.Failure(result.Error);

        var items = result.Value.AsEnumerable();

        if (filter?.IsActive is bool isActive)
            items = items.Where(c => c.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(filter?.Name))
        {
            var term = filter.Name.Trim();
            items = items.Where(c => c.Name.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter?.Cpf))
        {
            var cpfTerm = new string(filter.Cpf.Where(char.IsDigit).ToArray());
            items = items.Where(c => c.Cpf.Value.Contains(cpfTerm, StringComparison.Ordinal));
        }

        var page = Math.Max(1, filter?.Page ?? 1);
        var pageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 100);

        var projected = items
            .OrderBy(c => c.Name.Value)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(CustomerListItemDto.FromDomain)
            .ToList();

        return Result<IReadOnlyList<CustomerListItemDto>>.Success(projected);
    }

    public async Task<Result<IReadOnlyList<CustomerAttachmentOutputDto>>> ListAttachmentsAsync(
        TenantId tenantId,
        Guid customerId,
        CancellationToken ct = default)
    {
        var customerResult = await repository.GetById(customerId);
        if (customerResult.IsFailure)
            return Result<IReadOnlyList<CustomerAttachmentOutputDto>>.Failure(customerResult.Error);

        if (customerResult.Value.TenantId.Value != tenantId.Value)
            return Result<IReadOnlyList<CustomerAttachmentOutputDto>>.Failure("Customer does not belong to this tenant.");

        var attachmentsResult = await legacyPort.GetCustomerAttachmentsFromLegacyAsync(tenantId, customerId, ct);
        if (attachmentsResult.IsFailure)
            return Result<IReadOnlyList<CustomerAttachmentOutputDto>>.Failure(attachmentsResult.Error);

        var projected = attachmentsResult.Value
            .Select(CustomerAttachmentOutputDto.FromDomain)
            .ToList();

        return Result<IReadOnlyList<CustomerAttachmentOutputDto>>.Success(projected);
    }
}
