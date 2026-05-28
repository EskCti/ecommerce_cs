using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Queries;
using RetailOps.Core.Sales.Application.UseCases;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize]
public sealed class SalesController(
    ITenantContext tenantContext,
    IListSalesQuery listSalesQuery,
    CancelSaleUseCase cancelSaleUseCase) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? operatorUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new SaleListFilter(from, to, operatorUserId, page, pageSize);
        var result = await listSalesQuery.ListByTenantAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await cancelSaleUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
