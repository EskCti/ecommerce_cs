using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.MultiTenancy;
using RetailOps.Core.Returns.Application.DTOs;
using RetailOps.Core.Returns.Application.Queries;
using RetailOps.Core.Returns.Application.UseCases;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/returns/exchanges")]
[Authorize]
public sealed class ReturnsExchangesController(
    ITenantContext tenantContext,
    RegisterExchangeUseCase registerExchangeUseCase,
    DeleteExchangeUseCase deleteExchangeUseCase,
    IListExchangesQuery listExchangesQuery) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    private Guid OperatorUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new ExchangeListFilter(from, to, customerId, productId, page, pageSize);
        var result = await listExchangesQuery.ListByTenantAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterExchangeInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await registerExchangeUseCase.Execute((TenantId, OperatorUserId, request), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(List), new { id = result.Value.Id }, result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await deleteExchangeUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
