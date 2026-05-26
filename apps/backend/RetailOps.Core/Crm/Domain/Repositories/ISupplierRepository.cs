using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Domain.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Result<IEnumerable<Supplier>>> GetByTenantId(TenantId tenantId);
    Task<Result<bool>> TaxDocumentExists(
        TenantId tenantId,
        TaxDocument taxDocument,
        Guid? excludeSupplierId = null);
}
