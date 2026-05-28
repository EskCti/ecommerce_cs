using RetailOps.Core.Sales.Domain.Entities;

namespace RetailOps.Core.Sales.Application.DTOs;

public sealed record AddItemToCartInputDto
{
    public required string ScannedValue { get; init; }
    public decimal? UnitPriceOverride { get; init; }
}

public sealed record ConfirmGradeForItemInputDto
{
    public required IReadOnlyList<Guid> GradeOptionIds { get; init; }
}

public sealed record CartLineOutputDto
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required string Barcode { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal LineTotal { get; init; }
    public required string Status { get; init; }
    public required bool RequiresGrade { get; init; }
    public required IReadOnlyList<Guid> GradeOptionIds { get; init; }

    public static CartLineOutputDto FromDomain(SaleLine line) =>
        new()
        {
            Id = line.Id,
            ProductId = line.ProductId,
            Barcode = line.Barcode,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice.Value,
            LineTotal = line.CalculateTotal().Value,
            Status = line.Status.ToString(),
            RequiresGrade = line.RequiresGrade,
            GradeOptionIds = line.GradeOptionIds.ToList(),
        };
}
