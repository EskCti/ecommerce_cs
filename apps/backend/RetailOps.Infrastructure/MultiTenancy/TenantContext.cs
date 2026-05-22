using RetailOps.Core.MultiTenancy;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.MultiTenancy;

public sealed class TenantContext : ITenantContext, ITenantContextAccessor
{
    public TenantId TenantId { get; private set; } = TenantId.Platform;

    public bool HasTenant => !TenantId.IsPlatform;

    public void SetTenant(TenantId tenantId) => TenantId = tenantId;
}
