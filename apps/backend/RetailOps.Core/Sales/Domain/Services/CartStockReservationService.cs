using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Domain.Services;

public sealed class CartStockReservationService(IProductCatalogService catalogService)
{
    public Task<Result> ReserveLineAsync(TenantId tenantId, SaleLine line, CancellationToken ct = default)
    {
        if (line.GradeVariantId is Guid variantId)
            return catalogService.ReserveVariantStockAsync(tenantId, variantId, line.Quantity, ct);

        if (line.GradeOptionIds.Count > 0)
            return Task.FromResult(Result.Failure("Grade variant must be resolved before reserving stock."));

        return catalogService.ReserveStockAsync(tenantId, line.ProductId, line.Quantity, ct);
    }

    public Task<Result> ReleaseLineAsync(TenantId tenantId, SaleLine line, CancellationToken ct = default)
    {
        if (line.GradeVariantId is Guid variantId)
            return catalogService.ReleaseVariantStockAsync(tenantId, variantId, line.Quantity, ct);

        if (line.GradeOptionIds.Count > 0 && line.GradeVariantId is null)
            return Task.FromResult(Result.Success());

        return catalogService.ReleaseStockAsync(tenantId, line.ProductId, line.Quantity, ct);
    }

    public async Task<Result> ReleaseLinesAsync(
        TenantId tenantId,
        IEnumerable<SaleLine> lines,
        CancellationToken ct = default)
    {
        foreach (var line in lines)
        {
            if (line.Status != SaleLineStatus.Ready)
                continue;

            var release = await ReleaseLineAsync(tenantId, line, ct);
            if (release.IsFailure)
                return release;
        }

        return Result.Success();
    }
}
