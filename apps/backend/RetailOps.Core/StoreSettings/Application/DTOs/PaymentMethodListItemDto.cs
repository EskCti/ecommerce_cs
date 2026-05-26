namespace RetailOps.Core.StoreSettings.Application.DTOs;

public sealed record PaymentMethodListItemDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal SurchargePercent { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static PaymentMethodListItemDto FromDomain(Domain.Entities.PaymentMethod paymentMethod)
    {
        return new PaymentMethodListItemDto
        {
            Id = paymentMethod.Id,
            Name = paymentMethod.Name.Value,
            SurchargePercent = paymentMethod.SurchargePercent.Value,
            IsActive = paymentMethod.IsActive,
            CreatedAt = paymentMethod.CreatedAt,
            UpdatedAt = paymentMethod.UpdatedAt
        };
    }
}