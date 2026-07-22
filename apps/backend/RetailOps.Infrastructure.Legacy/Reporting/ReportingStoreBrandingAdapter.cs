using RetailOps.Core.Reporting.Application.DTOs;
using RetailOps.Core.Reporting.Application.Ports;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Reporting;

public sealed class ReportingStoreBrandingAdapter(IStoreConfigRepository storeConfigRepository)
    : IReportingStoreBrandingPort
{
    public async Task<Result<StoreBrandingDto>> GetBrandingAsync(TenantId tenantId, CancellationToken ct = default)
    {
        var configResult = await storeConfigRepository.GetByTenantId(tenantId);
        if (configResult.IsFailure)
            return Result<StoreBrandingDto>.Failure(configResult.Error);

        var config = configResult.Value;
        return Result<StoreBrandingDto>.Success(new StoreBrandingDto(
            config.Name.Value,
            config.LogoPath?.Value,
            config.ReportFormat.Value));
    }
}
