using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Domain.Entities;

public sealed class Commission : Entity
{
    public TenantId TenantId { get; private set; }
    public Guid SaleId { get; private set; }
    public int SellerLegacyId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public bool IsPaid { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public Guid? PaymentPayableId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Commission() { }

    private Commission(
        TenantId tenantId,
        Guid saleId,
        int sellerLegacyId,
        Money amount,
        bool isPaid,
        DateTime? paidAt,
        Guid? paymentPayableId)
    {
        TenantId = tenantId;
        SaleId = saleId;
        SellerLegacyId = sellerLegacyId;
        Amount = amount;
        IsPaid = isPaid;
        PaidAt = paidAt;
        PaymentPayableId = paymentPayableId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Commission> Create(
        TenantId tenantId,
        Guid saleId,
        int sellerLegacyId,
        Money amount)
    {
        if (saleId == Guid.Empty)
            return Result<Commission>.Failure("Sale id is required.");

        if (sellerLegacyId <= 0)
            return Result<Commission>.Failure("Seller is required.");

        return Result<Commission>.Success(new Commission(
            tenantId,
            saleId,
            sellerLegacyId,
            amount,
            isPaid: false,
            paidAt: null,
            paymentPayableId: null));
    }

    public static Result<Commission> Reconstitute(
        Guid id,
        TenantId tenantId,
        Guid saleId,
        int sellerLegacyId,
        Money amount,
        bool isPaid,
        DateTime? paidAt,
        Guid? paymentPayableId,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var commission = new Commission(tenantId, saleId, sellerLegacyId, amount, isPaid, paidAt, paymentPayableId)
        {
            Id = id,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        return Result<Commission>.Success(commission);
    }

    public Result MarkPaid(Guid paymentPayableId, DateTime paidAtUtc)
    {
        if (IsPaid)
            return Result.Failure("Commission is already paid.");

        if (paymentPayableId == Guid.Empty)
            return Result.Failure("Payment payable id is required.");

        IsPaid = true;
        PaidAt = paidAtUtc;
        PaymentPayableId = paymentPayableId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    internal void SyncIdentity(Guid id) => Id = id;
}
