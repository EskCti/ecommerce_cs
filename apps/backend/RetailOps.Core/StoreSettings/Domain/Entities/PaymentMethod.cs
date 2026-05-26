using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Domain.Entities;

public sealed class PaymentMethod : Entity
{
    public TenantId TenantId { get; private set; }
    public PaymentMethodName Name { get; private set; } = null!;
    public SurchargePercent SurchargePercent { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private PaymentMethod() { }

    private PaymentMethod(
        TenantId tenantId,
        PaymentMethodName name,
        SurchargePercent surchargePercent,
        bool isActive)
    {
        TenantId = tenantId;
        Name = name;
        SurchargePercent = surchargePercent;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<PaymentMethod> Create(
        TenantId tenantId,
        PaymentMethodName name,
        SurchargePercent surchargePercent,
        bool isActive = true)
    {
        return Result<PaymentMethod>.Success(new PaymentMethod(
            tenantId,
            name,
            surchargePercent,
            isActive));
    }

    public static Result<PaymentMethod> Reconstitute(
        Guid id,
        TenantId tenantId,
        PaymentMethodName name,
        SurchargePercent surchargePercent,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var paymentMethod = new PaymentMethod(tenantId, name, surchargePercent, isActive)
        {
            Id = id,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        return Result<PaymentMethod>.Success(paymentMethod);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public Result Rename(PaymentMethodName newName)
    {
        Name = newName;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UpdateSurcharge(SurchargePercent newSurchargePercent)
    {
        SurchargePercent = newSurchargePercent;
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