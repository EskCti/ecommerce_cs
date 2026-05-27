using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Application.UseCases;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/catalog/stock")]
[Authorize]
public sealed class CatalogStockController(
    ITenantContext tenantContext,
    RecordStockEntryUseCase recordStockEntryUseCase,
    RecordStockExitUseCase recordStockExitUseCase,
    PurchaseStockUseCase purchaseStockUseCase,
    IStockMovementRepository stockMovementRepository,
    IListLowStockProductsQuery lowStockProductsQuery) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet("movements")]
    public async Task<IActionResult> ListMovements([FromQuery] Guid productId, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await stockMovementRepository.GetByProductId(productId);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var projected = result.Value.Select(StockMovementOutputDto.FromDomain).ToList();
        return Ok(projected);
    }

    [HttpPost("movements")]
    public async Task<IActionResult> RecordEntry(
        [FromBody] RecordStockMovementInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await recordStockEntryUseCase.Execute((TenantId, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("movements/exit")]
    public async Task<IActionResult> RecordExit(
        [FromBody] RecordStockMovementInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await recordStockExitUseCase.Execute((TenantId, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseStock(
        [FromBody] PurchaseStockInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await purchaseStockUseCase.Execute((TenantId, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("low")]
    public async Task<IActionResult> ListLowStock(CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await lowStockProductsQuery.ExecuteAsync(tenantContext.TenantId, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
