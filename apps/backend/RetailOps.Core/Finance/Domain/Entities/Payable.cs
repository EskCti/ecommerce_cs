using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Domain.Entities;

public sealed class Payable : Entity
{
    public TenantId TenantId { get; private set; }
    public AccountType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = null!;
    public DueDate DueDate { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public DateTime? SettledAt { get; private set; }
    public Recurrence? Recurrence { get; private set; }
    public int? PersonLegacyId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? CommissionId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<FinanceAttachment> _attachments = [];
    public IReadOnlyList<FinanceAttachment> Attachments => _attachments.AsReadOnly();

    private Payable() { }

    private Payable(
        TenantId tenantId,
        AccountType type,
        string description,
        Money amount,
        DueDate dueDate,
        PaymentStatus status,
        Recurrence? recurrence,
        int? personLegacyId,
        Guid? productId,
        Guid? commissionId)
    {
        TenantId = tenantId;
        Type = type;
        Description = description;
        Amount = amount;
        DueDate = dueDate;
        Status = status;
        Recurrence = recurrence;
        PersonLegacyId = personLegacyId;
        ProductId = productId;
        CommissionId = commissionId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Payable> CreateExpense(
        TenantId tenantId,
        string description,
        Money amount,
        DueDate dueDate,
        Recurrence? recurrence = null,
        int? personLegacyId = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result<Payable>.Failure("Description is required.");

        return Result<Payable>.Success(new Payable(
            tenantId,
            AccountType.Expense,
            description.Trim(),
            amount,
            dueDate,
            PaymentStatus.Open,
            recurrence,
            personLegacyId,
            productId: null,
            commissionId: null));
    }

    public static Result<Payable> CreatePurchase(
        TenantId tenantId,
        string description,
        Money amount,
        DueDate dueDate,
        Guid productId,
        int? supplierLegacyId,
        DateTime referenceUtc)
    {
        if (productId == Guid.Empty)
            return Result<Payable>.Failure("Product id is required.");

        var status = dueDate.IsFuture(referenceUtc) ? PaymentStatus.Open : PaymentStatus.Settled;
        var settledAt = status == PaymentStatus.Settled ? referenceUtc : (DateTime?)null;

        var payable = new Payable(
            tenantId,
            AccountType.Purchase,
            description.Trim(),
            amount,
            dueDate,
            status,
            recurrence: null,
            supplierLegacyId,
            productId,
            commissionId: null);

        if (settledAt is not null)
            payable.SettledAt = settledAt;

        return Result<Payable>.Success(payable);
    }

    public static Result<Payable> CreateCommissionPayment(
        TenantId tenantId,
        Money amount,
        DueDate dueDate,
        Guid commissionId,
        int sellerLegacyId)
    {
        if (commissionId == Guid.Empty)
            return Result<Payable>.Failure("Commission id is required.");

        return Result<Payable>.Success(new Payable(
            tenantId,
            AccountType.CommissionPayment,
            "Pagamento de comissão",
            amount,
            dueDate,
            PaymentStatus.Settled,
            recurrence: null,
            sellerLegacyId,
            productId: null,
            commissionId));
    }

    public static Result<Payable> Reconstitute(
        Guid id,
        TenantId tenantId,
        AccountType type,
        string description,
        Money amount,
        DueDate dueDate,
        PaymentStatus status,
        DateTime? settledAt,
        Recurrence? recurrence,
        int? personLegacyId,
        Guid? productId,
        Guid? commissionId,
        DateTime createdAt,
        DateTime updatedAt,
        IEnumerable<FinanceAttachment>? attachments = null)
    {
        var payable = new Payable(
            tenantId,
            type,
            description,
            amount,
            dueDate,
            status,
            recurrence,
            personLegacyId,
            productId,
            commissionId)
        {
            Id = id,
            SettledAt = settledAt,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        if (attachments is not null)
            payable._attachments.AddRange(attachments);

        return Result<Payable>.Success(payable);
    }

    public Result Update(string description, Money amount, DueDate dueDate, Recurrence? recurrence, int? personLegacyId)
    {
        if (Status == PaymentStatus.Settled)
            return Result.Failure("Cannot update a settled payable.");

        if (Type != AccountType.Expense)
            return Result.Failure("Only expense payables can be updated.");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure("Description is required.");

        Description = description.Trim();
        Amount = amount;
        DueDate = dueDate;
        Recurrence = recurrence;
        PersonLegacyId = personLegacyId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddAttachment(FinanceAttachment attachment)
    {
        if (attachment.OwnerId != Id && Id != Guid.Empty)
            return Result.Failure("Attachment does not belong to this payable.");

        if (attachment.TenantId.Value != TenantId.Value)
            return Result.Failure("Attachment tenant mismatch.");

        _attachments.Add(attachment);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Settle(DateTime settlementDateUtc)
    {
        if (Status == PaymentStatus.Settled)
            return Result.Failure("Payable is already settled.");

        if (settlementDateUtc == default)
            return Result.Failure("Settlement date is required.");

        Status = PaymentStatus.Settled;
        SettledAt = settlementDateUtc;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    internal void SyncIdentity(Guid id) => Id = id;

    internal void SetAttachments(IEnumerable<FinanceAttachment> attachments)
    {
        _attachments.Clear();
        _attachments.AddRange(attachments);
    }
}
