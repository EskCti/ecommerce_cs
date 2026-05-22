using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.MultiTenancy;

public interface ITenantContext
{
    TenantId TenantId { get; }
    bool HasTenant { get; }
}
