using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Domain.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Result<Customer?>> GetByCpf(TenantId tenantId, Cpf cpf);
    Task<Result<bool>> CpfExists(TenantId tenantId, Cpf cpf, Guid? excludeCustomerId = null);
    Task<Result<IEnumerable<Customer>>> GetByTenantId(TenantId tenantId);
}
