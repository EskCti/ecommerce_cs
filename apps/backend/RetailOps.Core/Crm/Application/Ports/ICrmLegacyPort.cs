using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.Ports;

public interface ICrmLegacyPort
{
    Task<Result<IReadOnlyList<Customer>>> GetCustomersFromLegacyAsync(TenantId tenantId, CancellationToken ct = default);
    Task<Result<Customer?>> GetCustomerFromLegacyAsync(TenantId tenantId, Guid customerId, CancellationToken ct = default);
    Task<Result<Customer?>> GetCustomerByCpfFromLegacyAsync(TenantId tenantId, string cpf, CancellationToken ct = default);
    Task<Result<int>> SaveCustomerToLegacyAsync(Customer customer, CancellationToken ct = default);

    Task<Result<IReadOnlyList<CustomerAttachment>>> GetCustomerAttachmentsFromLegacyAsync(
        TenantId tenantId,
        Guid customerId,
        CancellationToken ct = default);
    Task<Result<int>> SaveCustomerAttachmentToLegacyAsync(CustomerAttachment attachment, CancellationToken ct = default);

    Task<Result<IReadOnlyList<Supplier>>> GetSuppliersFromLegacyAsync(TenantId tenantId, CancellationToken ct = default);
    Task<Result<Supplier?>> GetSupplierFromLegacyAsync(TenantId tenantId, Guid supplierId, CancellationToken ct = default);
    Task<Result<int>> SaveSupplierToLegacyAsync(Supplier supplier, CancellationToken ct = default);
}
