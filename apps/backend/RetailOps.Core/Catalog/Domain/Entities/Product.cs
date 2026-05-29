using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Domain.Entities;

public sealed class Product : Entity
{
    public TenantId TenantId { get; private set; }
    public Barcode Barcode { get; private set; } = null!;
    public ProductName Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public SalePrice SalePrice { get; private set; } = null!;
    public CostPrice CostPrice { get; private set; } = null!;
    public StockQuantity Stock { get; private set; } = null!;
    public ProfitMargin ProfitMargin { get; private set; } = null!;
    public StockAlertLevel StockAlertLevel { get; private set; } = null!;
    public CategoryId CategoryId { get; private set; } = null!;
    public Guid? SupplierId { get; private set; }
    public ProductPhotoPath? PhotoPath { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsOpenPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<GradeDimension> _gradeDimensions = [];
    public IReadOnlyList<GradeDimension> GradeDimensions => _gradeDimensions.AsReadOnly();

    private readonly List<GradeVariant> _gradeVariants = [];
    public IReadOnlyList<GradeVariant> GradeVariants => _gradeVariants.AsReadOnly();

    private Product() { }

    private Product(
        TenantId tenantId,
        Barcode barcode,
        ProductName name,
        string? description,
        SalePrice salePrice,
        CostPrice costPrice,
        StockQuantity stock,
        ProfitMargin profitMargin,
        StockAlertLevel stockAlertLevel,
        CategoryId categoryId,
        Guid? supplierId,
        ProductPhotoPath? photoPath,
        bool isActive)
    {
        TenantId = tenantId;
        Barcode = barcode;
        Name = name;
        Description = description;
        SalePrice = salePrice;
        CostPrice = costPrice;
        Stock = stock;
        ProfitMargin = profitMargin;
        StockAlertLevel = stockAlertLevel;
        CategoryId = categoryId;
        SupplierId = supplierId;
        PhotoPath = photoPath;
        IsActive = isActive;
        IsOpenPrice = salePrice.IsOpenPrice;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Product> Create(
        TenantId tenantId,
        Barcode barcode,
        ProductName name,
        SalePrice salePrice,
        CostPrice costPrice,
        StockQuantity stock,
        StockAlertLevel stockAlertLevel,
        CategoryId categoryId,
        string? description = null,
        Guid? supplierId = null,
        ProductPhotoPath? photoPath = null,
        bool isActive = true)
    {
        var marginResult = ProfitMarginCalculator.Calculate(salePrice, costPrice);
        if (marginResult.IsFailure)
            return Result<Product>.Failure(marginResult.Error);

        return Result<Product>.Success(new Product(
            tenantId,
            barcode,
            name,
            description,
            salePrice,
            costPrice,
            stock,
            marginResult.Value,
            stockAlertLevel,
            categoryId,
            supplierId,
            photoPath,
            isActive));
    }

    public static Result<Product> Reconstitute(
        Guid id,
        TenantId tenantId,
        Barcode barcode,
        ProductName name,
        string? description,
        SalePrice salePrice,
        CostPrice costPrice,
        StockQuantity stock,
        ProfitMargin profitMargin,
        StockAlertLevel stockAlertLevel,
        CategoryId categoryId,
        Guid? supplierId,
        ProductPhotoPath? photoPath,
        bool isActive,
        bool isOpenPrice,
        DateTime createdAt,
        DateTime updatedAt,
        IEnumerable<GradeDimension>? gradeDimensions = null,
        IEnumerable<GradeVariant>? gradeVariants = null)
    {
        var product = new Product(
            tenantId,
            barcode,
            name,
            description,
            salePrice,
            costPrice,
            stock,
            profitMargin,
            stockAlertLevel,
            categoryId,
            supplierId,
            photoPath,
            isActive)
        {
            Id = id,
            IsOpenPrice = isOpenPrice,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        if (gradeDimensions is not null)
            product._gradeDimensions.AddRange(gradeDimensions);

        if (gradeVariants is not null)
            product._gradeVariants.AddRange(gradeVariants);

        return Result<Product>.Success(product);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public Result UpdateDetails(
        ProductName? name = null,
        string? description = null,
        SalePrice? salePrice = null,
        CostPrice? costPrice = null,
        StockAlertLevel? stockAlertLevel = null,
        CategoryId? categoryId = null,
        Guid? supplierId = null,
        ProductPhotoPath? photoPath = null,
        bool updatePhoto = false,
        bool clearSupplier = false)
    {
        if (name is not null)
            Name = name;

        if (description is not null)
            Description = description;

        if (salePrice is not null)
        {
            SalePrice = salePrice;
            IsOpenPrice = salePrice.IsOpenPrice;
        }

        if (costPrice is not null)
            CostPrice = costPrice;

        if (salePrice is not null || costPrice is not null)
        {
            var marginResult = ProfitMarginCalculator.Calculate(SalePrice, CostPrice);
            if (marginResult.IsFailure)
                return Result.Failure(marginResult.Error);
            ProfitMargin = marginResult.Value;
        }

        if (stockAlertLevel is not null)
            StockAlertLevel = stockAlertLevel;

        if (categoryId is not null)
            CategoryId = categoryId;

        if (clearSupplier)
            SupplierId = null;
        else if (supplierId is not null)
            SupplierId = supplierId;

        if (updatePhoto)
            PhotoPath = photoPath;

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AdjustStock(StockQuantity newStock, StockAdjustmentPolicy policy)
    {
        var validation = policy.ValidateAdjustment(Stock, newStock, isExit: newStock.Value < Stock.Value);
        if (validation.IsFailure)
            return validation;

        Stock = newStock;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AddGradeDimension(GradeDimension dimension)
    {
        if (dimension.ProductId != Id && Id != Guid.Empty)
            return Result.Failure("Grade dimension does not belong to this product.");

        if (_gradeDimensions.Count >= GradeVariantSyncService.MaxDimensions)
            return Result.Failure($"A product may have at most {GradeVariantSyncService.MaxDimensions} grade dimensions.");

        _gradeDimensions.Add(dimension);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveGradeDimension(Guid dimensionId)
    {
        var dimension = _gradeDimensions.FirstOrDefault(d => d.Id == dimensionId);
        if (dimension is null)
            return Result.Failure("Grade dimension not found.");

        _gradeDimensions.Remove(dimension);
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

    internal void SetGradeDimensions(IEnumerable<GradeDimension> dimensions)
    {
        _gradeDimensions.Clear();
        _gradeDimensions.AddRange(dimensions);
    }

    internal void SetGradeVariants(IEnumerable<GradeVariant> variants)
    {
        _gradeVariants.Clear();
        _gradeVariants.AddRange(variants);
    }

    internal void ClearGradeVariants() => _gradeVariants.Clear();

    public Result<GradeVariant> AddGradeVariant(IReadOnlyList<Guid> optionIds, StockQuantity stock)
    {
        if (_gradeVariants.Any(v => v.MatchesOptions(optionIds)))
            return Result<GradeVariant>.Failure("Grade variant already exists for this combination.");

        var variantResult = GradeVariant.Create(Id, optionIds, stock);
        if (variantResult.IsFailure)
            return variantResult;

        _gradeVariants.Add(variantResult.Value);
        UpdatedAt = DateTime.UtcNow;
        return variantResult;
    }

    public Result AdjustVariantStock(Guid variantId, StockQuantity newStock)
    {
        var variant = _gradeVariants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            return Result.Failure("Grade variant not found.");

        return variant.AdjustStock(newStock);
    }

    public Result RemoveGradeVariant(Guid variantId)
    {
        var variant = _gradeVariants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            return Result.Failure("Grade variant not found.");

        _gradeVariants.Remove(variant);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    internal void RemoveVariantsReferencingOption(Guid optionId)
    {
        _gradeVariants.RemoveAll(v => v.ReferencesOption(optionId));
        UpdatedAt = DateTime.UtcNow;
    }

    public Result<GradeVariant?> FindVariantById(Guid variantId) =>
        Result<GradeVariant?>.Success(_gradeVariants.FirstOrDefault(v => v.Id == variantId));

    public Result<GradeVariant?> FindVariantByOptions(IEnumerable<Guid> optionIds)
    {
        var match = _gradeVariants.FirstOrDefault(v => v.MatchesOptions(optionIds));
        return Result<GradeVariant?>.Success(match);
    }

    public bool HasTwoGradeDimensions() => _gradeDimensions.Count >= 2;

    public bool IsLowStock() => LowStockPolicy.IsLowStock(Stock, StockAlertLevel);

    /// <summary>
    /// Keeps product-level stock aligned with grade variants (2D) or options (1D) for legacy produtos.estoque.
    /// </summary>
    internal Result SyncAggregateStockFromGrades()
    {
        if (_gradeDimensions.Count == 0)
            return Result.Success();

        var total = _gradeVariants.Count > 0
            ? _gradeVariants.Sum(v => v.Stock.Value)
            : _gradeDimensions.Count == 1
                ? _gradeDimensions[0].Options.Sum(o => o.Stock.Value)
                : 0;

        var stockResult = StockQuantity.Create(total);
        if (stockResult.IsFailure)
            return stockResult;

        Stock = stockResult.Value;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
