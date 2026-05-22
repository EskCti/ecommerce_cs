using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.MultiTenancy;

public interface ITenantContextAccessor
{
    void SetTenant(TenantId tenantId);
}
