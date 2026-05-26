using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.Crm.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Domain.Entities;

public sealed class CustomerAttachment : Entity
{
    public TenantId TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public AttachmentName Name { get; private set; } = null!;
    public AttachmentPath Path { get; private set; } = null!;
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CustomerAttachment() { }

    private CustomerAttachment(
        TenantId tenantId,
        Guid customerId,
        AttachmentName name,
        AttachmentPath path,
        DateTime? expiresAt)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        Name = name;
        Path = path;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<CustomerAttachment> Create(
        TenantId tenantId,
        Guid customerId,
        AttachmentName name,
        AttachmentPath path,
        DateTime? expiresAt = null)
    {
        return Result<CustomerAttachment>.Success(new CustomerAttachment(
            tenantId,
            customerId,
            name,
            path,
            expiresAt));
    }

    public static Result<CustomerAttachment> Reconstitute(
        Guid id,
        TenantId tenantId,
        Guid customerId,
        AttachmentName name,
        AttachmentPath path,
        DateTime? expiresAt,
        DateTime createdAt)
    {
        var attachment = new CustomerAttachment(tenantId, customerId, name, path, expiresAt)
        {
            Id = id,
            CreatedAt = createdAt
        };

        return Result<CustomerAttachment>.Success(attachment);
    }

    internal void SyncIdentity(Guid id) => Id = id;
}
