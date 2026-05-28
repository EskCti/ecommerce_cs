using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Domain.Entities;

public sealed class Sale : Entity
{
    public TenantId TenantId { get; private set; }
    public Guid CashSessionId { get; private set; }
    public Guid OperatorUserId { get; private set; }
    public PaymentTerms PaymentTerms { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid PaymentMethodId { get; private set; }
    public decimal Subtotal { get; private set; }
    public Discount Discount { get; private set; } = null!;
    public decimal Total { get; private set; }
    public ChangeAmount Change { get; private set; } = null!;
    public decimal CommissionAmount { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private readonly List<SaleLine> _lines = [];
    public IReadOnlyList<SaleLine> Lines => _lines.AsReadOnly();

    private Sale() { }

    private Sale(
        TenantId tenantId,
        Guid cashSessionId,
        Guid operatorUserId,
        PaymentTerms paymentTerms,
        Guid? customerId,
        Guid paymentMethodId,
        decimal subtotal,
        Discount discount,
        decimal total,
        ChangeAmount change,
        decimal commissionAmount,
        IEnumerable<SaleLine> lines)
    {
        TenantId = tenantId;
        CashSessionId = cashSessionId;
        OperatorUserId = operatorUserId;
        PaymentTerms = paymentTerms;
        CustomerId = customerId;
        PaymentMethodId = paymentMethodId;
        Subtotal = subtotal;
        Discount = discount;
        Total = total;
        Change = change;
        CommissionAmount = commissionAmount;
        CompletedAt = DateTime.UtcNow;
        _lines.AddRange(lines);
    }

    public static Result<Sale> Create(
        TenantId tenantId,
        Guid cashSessionId,
        Guid operatorUserId,
        PaymentTerms paymentTerms,
        Guid? customerId,
        Guid paymentMethodId,
        decimal subtotal,
        Discount discount,
        decimal total,
        ChangeAmount change,
        decimal commissionAmount,
        IEnumerable<SaleLine> lines)
    {
        if (cashSessionId == Guid.Empty)
            return Result<Sale>.Failure("Cash session id is required.");

        if (paymentMethodId == Guid.Empty)
            return Result<Sale>.Failure("Payment method id is required.");

        var lineList = lines.ToList();
        if (lineList.Count == 0)
            return Result<Sale>.Failure("Sale must have at least one line.");

        return Result<Sale>.Success(new Sale(
            tenantId,
            cashSessionId,
            operatorUserId,
            paymentTerms,
            customerId,
            paymentMethodId,
            subtotal,
            discount,
            total,
            change,
            commissionAmount,
            lineList));
    }

    public static Result<Sale> Reconstitute(
        Guid id,
        TenantId tenantId,
        Guid cashSessionId,
        Guid operatorUserId,
        PaymentTerms paymentTerms,
        Guid? customerId,
        Guid paymentMethodId,
        decimal subtotal,
        Discount discount,
        decimal total,
        ChangeAmount change,
        decimal commissionAmount,
        bool isCancelled,
        DateTime completedAt,
        DateTime? cancelledAt,
        IEnumerable<SaleLine> lines)
    {
        var sale = new Sale(
            tenantId,
            cashSessionId,
            operatorUserId,
            paymentTerms,
            customerId,
            paymentMethodId,
            subtotal,
            discount,
            total,
            change,
            commissionAmount,
            lines)
        {
            Id = id,
            IsCancelled = isCancelled,
            CompletedAt = completedAt,
            CancelledAt = cancelledAt
        };

        return Result<Sale>.Success(sale);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    internal void SetLines(IEnumerable<SaleLine> lines)
    {
        _lines.Clear();
        _lines.AddRange(lines);
    }

    public Result Cancel()
    {
        if (IsCancelled)
            return Result.Failure("Sale is already cancelled.");

        IsCancelled = true;
        CancelledAt = DateTime.UtcNow;
        return Result.Success();
    }
}
