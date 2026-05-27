using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Infrastructure.Legacy.Mappers.Catalog;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Catalog;

public sealed class LegacyStockMovementRepository(
    ICatalogLegacyPort legacyPort,
    LegacySasDbContext db) : IStockMovementRepository
{
    public async Task<Result> Save(StockMovement movement)
    {
        var legacyId = LegacyCatalogIds.ParseLegacyId(movement.ProductId, "0009");
        if (legacyId is null)
            return Result.Failure("Invalid product id for stock movement.");

        var row = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == legacyId);

        if (row is null)
            return Result.Failure("Product not found for stock movement.");

        var tenantId = TenantId.Create(row.CompanyId);
        if (tenantId.IsFailure)
            return Result.Failure(tenantId.Error);

        var result = await legacyPort.SaveStockMovementToLegacyAsync(movement, tenantId.Value);
        return result.IsFailure ? Result.Failure(result.Error) : Result.Success();
    }

    public async Task<Result<IReadOnlyList<StockMovement>>> GetByProductId(Guid productId)
    {
        var result = await legacyPort.GetStockMovementsFromLegacyAsync(productId);
        return result.IsFailure
            ? Result<IReadOnlyList<StockMovement>>.Failure(result.Error)
            : Result<IReadOnlyList<StockMovement>>.Success(result.Value);
    }
}
