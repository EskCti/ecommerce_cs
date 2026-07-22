using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Notifications.Domain.Events;

public sealed record DigestSent(TenantId TenantId, DateOnly DigestDate, DateTime SentAtUtc);
