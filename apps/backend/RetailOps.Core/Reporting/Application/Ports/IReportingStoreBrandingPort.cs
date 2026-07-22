using RetailOps.Core.Reporting.Application.DTOs;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Reporting.Application.Ports;

public interface IReportingStoreBrandingPort
{
    Task<Result<StoreBrandingDto>> GetBrandingAsync(TenantId tenantId, CancellationToken ct = default);
}
