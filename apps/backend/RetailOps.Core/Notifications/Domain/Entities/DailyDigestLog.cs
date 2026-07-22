using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Notifications.Domain.Entities;

public sealed class DailyDigestLog : Entity
{
    public TenantId TenantId { get; private set; }
    public DateOnly DigestDate { get; private set; }
    public DateTime SentAt { get; private set; }

    private DailyDigestLog() { }

    public static Result<DailyDigestLog> Record(TenantId tenantId, DateOnly digestDate, DateTime sentAtUtc)
    {
        if (sentAtUtc == default)
            return Result<DailyDigestLog>.Failure("Sent timestamp is required.");

        return Result<DailyDigestLog>.Success(new DailyDigestLog
        {
            TenantId = tenantId,
            DigestDate = digestDate,
            SentAt = sentAtUtc,
        });
    }

    public static Result<DailyDigestLog> Reconstitute(
        Guid id,
        TenantId tenantId,
        DateOnly digestDate,
        DateTime sentAtUtc)
    {
        return Result<DailyDigestLog>.Success(new DailyDigestLog
        {
            Id = id,
            TenantId = tenantId,
            DigestDate = digestDate,
            SentAt = sentAtUtc,
        });
    }
}
