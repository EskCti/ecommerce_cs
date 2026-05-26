namespace RetailOps.Core.StoreSettings.Application.DTOs;

public sealed record PaymentMethodOutputDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required string Name { get; init; }
    public required decimal SurchargePercent { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static PaymentMethodOutputDto FromDomain(Domain.Entities.PaymentMethod paymentMethod)
    {
        return new PaymentMethodOutputDto
        {
            Id = paymentMethod.Id,
            TenantId = paymentMethod.TenantId.Value,
            Name = paymentMethod.Name.Value,
            SurchargePercent = paymentMethod.SurchargePercent.Value,
            IsActive = paymentMethod.IsActive,
            CreatedAt = paymentMethod.CreatedAt,
            UpdatedAt = paymentMethod.UpdatedAt
        };
    }
}