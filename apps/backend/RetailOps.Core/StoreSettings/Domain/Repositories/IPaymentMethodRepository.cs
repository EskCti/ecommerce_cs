using RetailOps.Shared.Kernel.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Domain.Repositories;

public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{
    Task<Result<IEnumerable<PaymentMethod>>> GetByTenantId(TenantId tenantId);
    Task<Result<PaymentMethod>> GetByName(TenantId tenantId, PaymentMethodName name);
    Task<Result<bool>> NameExists(TenantId tenantId, PaymentMethodName name, Guid? excludeId = null);
}