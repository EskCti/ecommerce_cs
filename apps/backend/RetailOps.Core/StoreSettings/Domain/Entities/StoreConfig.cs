using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Domain.Entities;

public sealed class StoreConfig : Entity
{
    public TenantId TenantId { get; private set; }
    public StoreName Name { get; private set; } = null!;
    public Cnpj? Cnpj { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public CommissionRate CommissionRate { get; private set; } = null!;
    public ReportFormat ReportFormat { get; private set; } = null!;
    public ApiToken? ApiToken { get; private set; }
    public ImagePath? LogoPath { get; private set; }
    public string? Contacts { get; private set; }
    public string? Address { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private StoreConfig() { }

    private StoreConfig(
        TenantId tenantId,
        StoreName name,
        Cnpj? cnpj,
        DiscountType discountType,
        decimal discountValue,
        CommissionRate commissionRate,
        ReportFormat reportFormat,
        ApiToken? apiToken,
        ImagePath? logoPath,
        string? contacts,
        string? address)
    {
        TenantId = tenantId;
        Name = name;
        Cnpj = cnpj;
        DiscountType = discountType;
        DiscountValue = discountValue;
        CommissionRate = commissionRate;
        ReportFormat = reportFormat;
        ApiToken = apiToken;
        LogoPath = logoPath;
        Contacts = contacts;
        Address = address;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<StoreConfig> Create(
        TenantId tenantId,
        StoreName name,
        Cnpj? cnpj,
        DiscountType discountType,
        decimal discountValue,
        CommissionRate commissionRate,
        ReportFormat reportFormat,
        ApiToken? apiToken = null,
        ImagePath? logoPath = null,
        string? contacts = null,
        string? address = null)
    {
        var validationResult = ValidateDiscount(discountType, discountValue);
        if (validationResult.IsFailure)
            return Result<StoreConfig>.Failure(validationResult.Error);

        return Result<StoreConfig>.Success(new StoreConfig(
            tenantId,
            name,
            cnpj,
            discountType,
            discountValue,
            commissionRate,
            reportFormat,
            apiToken,
            logoPath,
            contacts,
            address));
    }

    public static Result<StoreConfig> Reconstitute(
        Guid id,
        TenantId tenantId,
        StoreName name,
        Cnpj? cnpj,
        DiscountType discountType,
        decimal discountValue,
        CommissionRate commissionRate,
        ReportFormat reportFormat,
        ApiToken? apiToken,
        ImagePath? logoPath,
        string? contacts,
        string? address,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var validationResult = ValidateDiscount(discountType, discountValue);
        if (validationResult.IsFailure)
            return Result<StoreConfig>.Failure(validationResult.Error);

        var config = new StoreConfig(
            tenantId,
            name,
            cnpj,
            discountType,
            discountValue,
            commissionRate,
            reportFormat,
            apiToken,
            logoPath,
            contacts,
            address)
        {
            Id = id,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        return Result<StoreConfig>.Success(config);
    }

    public Result UpdateGeneralInfo(
        StoreName name,
        Cnpj? cnpj,
        ImagePath? logoPath,
        string? contacts,
        string? address)
    {
        Name = name;
        Cnpj = cnpj;
        LogoPath = logoPath;
        Contacts = contacts;
        Address = address;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UpdateDiscountSettings(DiscountType discountType, decimal discountValue)
    {
        var validationResult = ValidateDiscount(discountType, discountValue);
        if (validationResult.IsFailure)
            return validationResult;

        DiscountType = discountType;
        DiscountValue = discountValue;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UpdateReportSettings(CommissionRate commissionRate, ReportFormat reportFormat)
    {
        CommissionRate = commissionRate;
        ReportFormat = reportFormat;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UpdateIntegrationToken(ApiToken? apiToken)
    {
        ApiToken = apiToken;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    internal void SyncIdentity(Guid id) => Id = id;

    private static Result ValidateDiscount(DiscountType discountType, decimal discountValue)
    {
        if (discountType == DiscountType.None && discountValue != 0)
            return Result.Failure("Discount value must be 0 when discount type is None.");

        if (discountType == DiscountType.Percentage && (discountValue < 0 || discountValue > 100))
            return Result.Failure("Percentage discount must be between 0 and 100.");

        if (discountType == DiscountType.FixedAmount && discountValue < 0)
            return Result.Failure("Fixed amount discount cannot be negative.");

        return Result.Success();
    }
}