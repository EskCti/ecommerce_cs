using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Domain.Entities;

public sealed class FinanceAttachment : Entity
{
    public TenantId TenantId { get; private set; }
    public Guid OwnerId { get; private set; }
    public AttachmentMeta Meta { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private FinanceAttachment() { }

    private FinanceAttachment(TenantId tenantId, Guid ownerId, AttachmentMeta meta)
    {
        TenantId = tenantId;
        OwnerId = ownerId;
        Meta = meta;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<FinanceAttachment> Create(TenantId tenantId, Guid ownerId, AttachmentMeta meta)
    {
        if (ownerId == Guid.Empty)
            return Result<FinanceAttachment>.Failure("Owner id is required.");

        return Result<FinanceAttachment>.Success(new FinanceAttachment(tenantId, ownerId, meta));
    }

    public static Result<FinanceAttachment> Reconstitute(
        Guid id,
        TenantId tenantId,
        Guid ownerId,
        AttachmentMeta meta,
        DateTime createdAt)
    {
        var attachment = new FinanceAttachment(tenantId, ownerId, meta)
        {
            Id = id,
            CreatedAt = createdAt
        };

        return Result<FinanceAttachment>.Success(attachment);
    }
}
