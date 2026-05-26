namespace RetailOps.Core.StoreSettings.Application.DTOs;

public sealed record CreatePaymentMethodInputDto
{
    public required string Name { get; init; }
    public required decimal SurchargePercent { get; init; }
    public bool IsActive { get; init; } = true;
}