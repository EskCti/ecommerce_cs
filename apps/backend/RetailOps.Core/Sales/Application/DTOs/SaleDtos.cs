using RetailOps.Core.Sales.Domain.Entities;

namespace RetailOps.Core.Sales.Application.DTOs;

public sealed record FinalizeSaleInputDto
{
    public required Guid PaymentMethodId { get; init; }
    public required string PaymentTerms { get; init; }
    public Guid? CustomerId { get; init; }
    public required decimal AmountPaid { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal? SellerCommissionPercent { get; init; }
}

public sealed record SaleOutputDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required Guid CashSessionId { get; init; }
    public required Guid OperatorUserId { get; init; }
    public required string PaymentTerms { get; init; }
    public Guid? CustomerId { get; init; }
    public required Guid PaymentMethodId { get; init; }
    public required decimal Subtotal { get; init; }
    public required decimal Discount { get; init; }
    public required decimal Total { get; init; }
    public required decimal Change { get; init; }
    public required decimal CommissionAmount { get; init; }
    public required bool IsCancelled { get; init; }
    public required DateTime CompletedAt { get; init; }
    public DateTime? CancelledAt { get; init; }
    public required IReadOnlyList<CartLineOutputDto> Lines { get; init; }

    public static SaleOutputDto FromDomain(Sale sale) =>
        new()
        {
            Id = sale.Id,
            TenantId = sale.TenantId.Value,
            CashSessionId = sale.CashSessionId,
            OperatorUserId = sale.OperatorUserId,
            PaymentTerms = sale.PaymentTerms.ToString(),
            CustomerId = sale.CustomerId,
            PaymentMethodId = sale.PaymentMethodId,
            Subtotal = sale.Subtotal,
            Discount = sale.Discount.Value,
            Total = sale.Total,
            Change = sale.Change.Value,
            CommissionAmount = sale.CommissionAmount,
            IsCancelled = sale.IsCancelled,
            CompletedAt = sale.CompletedAt,
            CancelledAt = sale.CancelledAt,
            Lines = sale.Lines.Select(CartLineOutputDto.FromDomain).ToList(),
        };
}

public sealed record SaleListFilter(
    DateTime? From,
    DateTime? To,
    Guid? OperatorUserId,
    int Page = 1,
    int PageSize = 20);

public sealed record SaleListItemDto(
    Guid Id,
    decimal Total,
    string PaymentTerms,
    bool IsCancelled,
    DateTime CompletedAt);

public sealed record SaleListPageDto(
    IReadOnlyList<SaleListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
