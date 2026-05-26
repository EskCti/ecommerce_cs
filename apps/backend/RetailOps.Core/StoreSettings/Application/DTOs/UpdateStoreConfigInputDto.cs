namespace RetailOps.Core.StoreSettings.Application.DTOs;

public sealed record UpdateStoreConfigInputDto
{
    public string? Name { get; init; }
    public string? Cnpj { get; init; }
    public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public decimal? CommissionRate { get; init; }
    public string? ReportFormat { get; init; }
    public string? ApiToken { get; init; }
    public string? LogoPath { get; init; }
    public string? Contacts { get; init; }
    public string? Address { get; init; }
}