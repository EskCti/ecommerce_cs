using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Domain.Entities;

public sealed class Receivable : Entity
{
    public TenantId TenantId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = null!;
    public DueDate DueDate { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public DateTime? SettledAt { get; private set; }
    public Guid? SaleId { get; private set; }
    public int? PersonLegacyId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<FinanceAttachment> _attachments = [];
    public IReadOnlyList<FinanceAttachment> Attachments => _attachments.AsReadOnly();

    private Receivable() { }

    private Receivable(
        TenantId tenantId,
        string description,
        Money amount,
        DueDate dueDate,
        PaymentStatus status,
        Guid? saleId,
        int? personLegacyId)
    {
        TenantId = tenantId;
        Description = description;
        Amount = amount;
        DueDate = dueDate;
        Status = status;
        SaleId = saleId;
        PersonLegacyId = personLegacyId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Receivable> CreateManual(
        TenantId tenantId,
        string description,
        Money amount,
        DueDate dueDate,
        int? personLegacyId = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result<Receivable>.Failure("Description is required.");

        return Result<Receivable>.Success(new Receivable(
            tenantId,
            description.Trim(),
            amount,
            dueDate,
            PaymentStatus.Open,
            saleId: null,
            personLegacyId));
    }

    public static Result<Receivable> CreateFromSale(
        TenantId tenantId,
        Guid saleId,
        Money amount,
        DueDate dueDate,
        int? personLegacyId = null)
    {
        if (saleId == Guid.Empty)
            return Result<Receivable>.Failure("Sale id is required.");

        return Result<Receivable>.Success(new Receivable(
            tenantId,
            "Venda PDV",
            amount,
            dueDate,
            PaymentStatus.Open,
            saleId,
            personLegacyId));
    }

    public static Result<Receivable> Reconstitute(
        Guid id,
        TenantId tenantId,
        string description,
        Money amount,
        DueDate dueDate,
        PaymentStatus status,
        DateTime? settledAt,
        Guid? saleId,
        int? personLegacyId,
        DateTime createdAt,
        DateTime updatedAt,
        IEnumerable<FinanceAttachment>? attachments = null)
    {
        var receivable = new Receivable(tenantId, description, amount, dueDate, status, saleId, personLegacyId)
        {
            Id = id,
            SettledAt = settledAt,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        if (attachments is not null)
            receivable._attachments.AddRange(attachments);

        return Result<Receivable>.Success(receivable);
    }

    public Result Update(string description, Money amount, DueDate dueDate, int? personLegacyId)
    {
        if (Status == PaymentStatus.Settled)
            return Result.Failure("Cannot update a settled receivable.");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure("Description is required.");

        Description = description.Trim();
        Amount = amount;
        DueDate = dueDate;
        PersonLegacyId = personLegacyId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddAttachment(FinanceAttachment attachment)
    {
        if (attachment.OwnerId != Id && Id != Guid.Empty)
            return Result.Failure("Attachment does not belong to this receivable.");

        if (attachment.TenantId.Value != TenantId.Value)
            return Result.Failure("Attachment tenant mismatch.");

        _attachments.Add(attachment);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Settle(DateTime settlementDateUtc)
    {
        if (Status == PaymentStatus.Settled)
            return Result.Failure("Receivable is already settled.");

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
