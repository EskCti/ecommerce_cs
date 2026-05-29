using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.Entities;

public sealed class SaleLine : Entity
{
    public Guid ProductId { get; private set; }
    public string Barcode { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public UnitPrice UnitPrice { get; private set; } = null!;
    public IReadOnlyList<Guid> GradeOptionIds => _gradeOptionIds.AsReadOnly();
    public Guid? GradeVariantId { get; private set; }
    public SaleLineStatus Status { get; private set; }
    public bool RequiresGrade { get; private set; }

    private readonly List<Guid> _gradeOptionIds = [];

    private SaleLine() { }

    private SaleLine(
        Guid productId,
        string barcode,
        int quantity,
        UnitPrice unitPrice,
        bool requiresGrade,
        IEnumerable<Guid>? gradeOptionIds,
        SaleLineStatus status,
        Guid? gradeVariantId = null)
    {
        ProductId = productId;
        Barcode = barcode;
        Quantity = quantity;
        UnitPrice = unitPrice;
        RequiresGrade = requiresGrade;
        Status = status;
        GradeVariantId = gradeVariantId;

        if (gradeOptionIds is not null)
            _gradeOptionIds.AddRange(gradeOptionIds);
    }

    public static Result<SaleLine> Create(
        Guid productId,
        string barcode,
        int quantity,
        UnitPrice unitPrice,
        bool requiresGrade)
    {
        if (productId == Guid.Empty)
            return Result<SaleLine>.Failure("Product id is required.");

        if (string.IsNullOrWhiteSpace(barcode))
            return Result<SaleLine>.Failure("Barcode is required.");

        if (quantity <= 0)
            return Result<SaleLine>.Failure("Quantity must be greater than zero.");

        var status = requiresGrade ? SaleLineStatus.PendingGrade : SaleLineStatus.Ready;

        return Result<SaleLine>.Success(new SaleLine(
            productId,
            barcode.Trim(),
            quantity,
            unitPrice,
            requiresGrade,
            null,
            status));
    }

    public static Result<SaleLine> Reconstitute(
        Guid id,
        Guid productId,
        string barcode,
        int quantity,
        UnitPrice unitPrice,
        bool requiresGrade,
        SaleLineStatus status,
        IEnumerable<Guid> gradeOptionIds,
        Guid? gradeVariantId = null)
    {
        var line = new SaleLine(productId, barcode, quantity, unitPrice, requiresGrade, gradeOptionIds, status, gradeVariantId)
        {
            Id = id
        };

        return Result<SaleLine>.Success(line);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    internal void SetRequiresGrade(bool requiresGrade) => RequiresGrade = requiresGrade;

    public Result ConfirmGrade(IEnumerable<Guid> gradeOptionIds, Guid? gradeVariantId = null)
    {
        if (!RequiresGrade)
            return Result.Failure("Line does not require grade confirmation.");

        if (gradeVariantId is Guid variantId && variantId != Guid.Empty)
        {
            GradeVariantId = variantId;
            _gradeOptionIds.Clear();
            if (gradeOptionIds is not null)
                _gradeOptionIds.AddRange(gradeOptionIds);
            Status = SaleLineStatus.Ready;
            return Result.Success();
        }

        var options = gradeOptionIds?.ToList() ?? [];
        if (options.Count == 0)
            return Result.Failure("At least one grade option or grade variant id is required.");

        _gradeOptionIds.Clear();
        _gradeOptionIds.AddRange(options);
        GradeVariantId = null;
        Status = SaleLineStatus.Ready;

        return Result.Success();
    }

    public SaleLineTotal CalculateTotal() =>
        SaleLineTotal.FromQuantityAndUnitPrice(Quantity, UnitPrice).Value!;
}
