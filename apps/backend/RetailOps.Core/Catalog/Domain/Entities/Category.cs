using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.Catalog.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Domain.Entities;

public sealed class Category : Entity
{
    public TenantId TenantId { get; private set; }
    public ProductName Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Category() { }

    private Category(TenantId tenantId, ProductName name, bool isActive)
    {
        TenantId = tenantId;
        Name = name;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Category> Create(TenantId tenantId, ProductName name, bool isActive = true) =>
        Result<Category>.Success(new Category(tenantId, name, isActive));

    public static Result<Category> Reconstitute(
        Guid id,
        TenantId tenantId,
        ProductName name,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var category = new Category(tenantId, name, isActive)
        {
            Id = id,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        return Result<Category>.Success(category);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public Result UpdateName(ProductName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
