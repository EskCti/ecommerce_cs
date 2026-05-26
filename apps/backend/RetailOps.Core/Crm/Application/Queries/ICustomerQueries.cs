using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.Queries;

public interface ICustomerQueries
{
    Task<Result<IReadOnlyList<CustomerListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        CustomerListFilter? filter = null,
        CancellationToken ct = default);

    Task<Result<IReadOnlyList<CustomerAttachmentOutputDto>>> ListAttachmentsAsync(
        TenantId tenantId,
        Guid customerId,
        CancellationToken ct = default);
}

public sealed record CustomerListFilter(
    string? Name,
    string? Cpf,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20);

public interface ISupplierQueries
{
    Task<Result<IReadOnlyList<SupplierListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        SupplierListFilter? filter = null,
        CancellationToken ct = default);
}

public sealed record SupplierListFilter(
    string? Name,
    PersonType? PersonType,
    string? TaxDocument,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20);
