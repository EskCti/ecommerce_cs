using RetailOps.Core.Returns.Domain.Entities;
using RetailOps.Core.Returns.Domain.ValueObjects;

namespace RetailOps.Core.Returns.Application.DTOs;

public sealed record RegisterExchangeInputDto
{
    public Guid? CustomerId { get; init; }
    public string? Cpf { get; init; }
    public string? CustomerName { get; init; }
    public Guid ProductInId { get; init; }
    public Guid ProductOutId { get; init; }
    public Guid? GradeVariantInId { get; init; }
    public Guid? GradeVariantOutId { get; init; }
    public IReadOnlyList<Guid>? GradeOptionIdsIn { get; init; }
    public IReadOnlyList<Guid>? GradeOptionIdsOut { get; init; }
}

public sealed record ExchangeOutputDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ProductInId { get; init; }
    public Guid ProductOutId { get; init; }
    public Guid? GradeVariantInId { get; init; }
    public Guid? GradeVariantOutId { get; init; }
    public IReadOnlyList<Guid> GradeOptionIdsIn { get; init; } = [];
    public IReadOnlyList<Guid> GradeOptionIdsOut { get; init; } = [];
    public int Quantity { get; init; }
    public Guid OperatorUserId { get; init; }
    public DateTime RegisteredAt { get; init; }

    public static ExchangeOutputDto FromDomain(Exchange exchange) =>
        new()
        {
            Id = exchange.Id,
            CustomerId = exchange.CustomerId.Value,
            ProductInId = exchange.ProductInId,
            ProductOutId = exchange.ProductOutId,
            GradeVariantInId = exchange.GradeIn?.VariantId,
            GradeVariantOutId = exchange.GradeOut?.VariantId,
            GradeOptionIdsIn = exchange.GradeIn?.OptionIds.ToArray() ?? [],
            GradeOptionIdsOut = exchange.GradeOut?.OptionIds.ToArray() ?? [],
            Quantity = exchange.Quantity.Value,
            OperatorUserId = exchange.OperatorUserId,
            RegisteredAt = exchange.RegisteredAt
        };
}

public sealed record ExchangeListFilter(
    DateTime? From,
    DateTime? To,
    Guid? CustomerId,
    Guid? ProductId,
    int Page,
    int PageSize);

public sealed record ExchangeListItemDto(
    Guid Id,
    Guid CustomerId,
    Guid ProductInId,
    Guid ProductOutId,
    DateTime RegisteredAt);

public sealed record ExchangeListPageDto(
    IReadOnlyList<ExchangeListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
