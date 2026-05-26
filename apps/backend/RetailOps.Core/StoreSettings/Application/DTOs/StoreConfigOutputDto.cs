using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.DTOs;

public sealed record StoreConfigOutputDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required string Name { get; init; }
    public string? Cnpj { get; init; }
    public required string DiscountType { get; init; }
    public required decimal DiscountValue { get; init; }
    public required decimal CommissionRate { get; init; }
    public required string ReportFormat { get; init; }
    public string? ApiToken { get; init; }
    public string? LogoPath { get; init; }
    public string? Contacts { get; init; }
    public string? Address { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static StoreConfigOutputDto FromDomain(Domain.Entities.StoreConfig storeConfig)
    {
        return new StoreConfigOutputDto
        {
            Id = storeConfig.Id,
            TenantId = storeConfig.TenantId.Value,
            Name = storeConfig.Name.Value,
            Cnpj = storeConfig.Cnpj?.Value,
            DiscountType = storeConfig.DiscountType.ToString(),
            DiscountValue = storeConfig.DiscountValue,
            CommissionRate = storeConfig.CommissionRate.Value,
            ReportFormat = storeConfig.ReportFormat.Value,
            ApiToken = storeConfig.ApiToken?.Value,
            LogoPath = storeConfig.LogoPath?.Value,
            Contacts = storeConfig.Contacts,
            Address = storeConfig.Address,
            CreatedAt = storeConfig.CreatedAt,
            UpdatedAt = storeConfig.UpdatedAt
        };
    }
}