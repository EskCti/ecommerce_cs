using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Infrastructure.Legacy.Mappers.Catalog;

public static class LegacyStockMovementMapper
{
    public static Result<StockMovement> ToDomain(LegacyStockEntryRow row, Guid productId)
    {
        var quantityResult = StockQuantity.Create(row.Quantity);
        if (quantityResult.IsFailure)
            return Result<StockMovement>.Failure(quantityResult.Error);

        var type = row.MovementType == nameof(StockMovementType.Purchase)
            ? StockMovementType.Purchase
            : StockMovementType.Entry;

        return StockMovement.Reconstitute(
            LegacyCatalogIds.StockEntry(row.Id),
            productId,
            quantityResult.Value,
            row.Reason ?? string.Empty,
            row.UserId,
            type,
            row.CreatedAt);
    }

    public static Result<StockMovement> ToDomain(LegacyStockExitRow row, Guid productId)
    {
        var quantityResult = StockQuantity.Create(row.Quantity);
        if (quantityResult.IsFailure)
            return Result<StockMovement>.Failure(quantityResult.Error);

        return StockMovement.Reconstitute(
            LegacyCatalogIds.StockExit(row.Id),
            productId,
            quantityResult.Value,
            row.Reason ?? string.Empty,
            row.UserId,
            StockMovementType.Exit,
            row.CreatedAt);
    }

    public static Result<LegacyStockEntryRow> ToEntryLegacy(
        StockMovement movement,
        int productLegacyId,
        int companyId,
        int? legacyId = null)
    {
        var row = new LegacyStockEntryRow
        {
            Id = legacyId ?? LegacyCatalogIds.ParseLegacyId(movement.Id, "0013") ?? 0,
            CompanyId = companyId,
            ProductLegacyId = productLegacyId,
            Quantity = movement.Quantity.Value,
            Reason = movement.Reason,
            UserId = movement.UserId,
            CreatedAt = movement.CreatedAt
        };

        return Result<LegacyStockEntryRow>.Success(row);
    }

    public static Result<LegacyStockExitRow> ToExitLegacy(
        StockMovement movement,
        int productLegacyId,
        int companyId,
        int? legacyId = null)
    {
        var row = new LegacyStockExitRow
        {
            Id = legacyId ?? LegacyCatalogIds.ParseLegacyId(movement.Id, "0014") ?? 0,
            CompanyId = companyId,
            ProductLegacyId = productLegacyId,
            Quantity = movement.Quantity.Value,
            Reason = movement.Reason,
            UserId = movement.UserId,
            CreatedAt = movement.CreatedAt,
            MovementType = movement.Type.ToString()
        };

        return Result<LegacyStockExitRow>.Success(row);
    }
}
