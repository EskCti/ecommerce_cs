using RetailOps.Core.Notifications.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Notifications.Domain.Repositories;

public interface IDailyDigestLogRepository
{
    Task<Result<bool>> ExistsForDateAsync(TenantId tenantId, DateOnly digestDate, CancellationToken ct = default);
    Task<Result> SaveAsync(DailyDigestLog log, CancellationToken ct = default);
}
