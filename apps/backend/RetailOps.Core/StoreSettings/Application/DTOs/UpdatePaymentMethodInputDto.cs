namespace RetailOps.Core.StoreSettings.Application.DTOs;

public sealed record UpdatePaymentMethodInputDto
{
    public string? Name { get; init; }
    public decimal? SurchargePercent { get; init; }
    public bool? IsActive { get; init; }
}