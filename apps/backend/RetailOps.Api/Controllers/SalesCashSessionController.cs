using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.UseCases;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/sales/cash-session")]
[Authorize]
public sealed class SalesCashSessionController(
    ITenantContext tenantContext,
    OpenCashSessionUseCase openCashSessionUseCase,
    GetCurrentCashSessionUseCase getCurrentCashSessionUseCase,
    AddItemToCartUseCase addItemToCartUseCase,
    ConfirmGradeForItemUseCase confirmGradeForItemUseCase,
    RemoveCartLineUseCase removeCartLineUseCase,
    RegisterCashWithdrawalUseCase registerCashWithdrawalUseCase,
    CloseCashSessionUseCase closeCashSessionUseCase,
    FinalizeSaleUseCase finalizeSaleUseCase) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    private Guid OperatorUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;

    [HttpPost("open")]
    public async Task<IActionResult> Open([FromBody] OpenCashSessionInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await openCashSessionUseCase.Execute((TenantId, OperatorUserId, request), ct);
        if (result.IsFailure)
        {
            if (result.Error.Contains("PIN", StringComparison.OrdinalIgnoreCase))
                return StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetCurrent), result.Value);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await getCurrentCashSessionUseCase.Execute((TenantId, OperatorUserId), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        if (result.Value is null)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpPost("current/cart/items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemToCartInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await addItemToCartUseCase.Execute((TenantId, OperatorUserId, request), ct);
        if (result.IsFailure)
        {
            if (result.Error.Contains("No open cash session", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("current/cart/items/{lineId:guid}/confirm-grade")]
    public async Task<IActionResult> ConfirmGrade(
        Guid lineId,
        [FromBody] ConfirmGradeForItemInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await confirmGradeForItemUseCase.Execute((TenantId, OperatorUserId, lineId, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpDelete("current/cart/items/{lineId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid lineId, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await removeCartLineUseCase.Execute((TenantId, OperatorUserId, lineId), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("withdrawal")]
    public async Task<IActionResult> Withdrawal([FromBody] RegisterCashWithdrawalInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await registerCashWithdrawalUseCase.Execute((TenantId, OperatorUserId, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("close")]
    public async Task<IActionResult> Close([FromBody] CloseCashSessionInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await closeCashSessionUseCase.Execute((TenantId, OperatorUserId, request), ct);
        if (result.IsFailure)
        {
            if (result.Error.Contains("PIN", StringComparison.OrdinalIgnoreCase))
                return StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("finalize")]
    public async Task<IActionResult> Finalize([FromBody] FinalizeSaleInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await finalizeSaleUseCase.Execute((TenantId, OperatorUserId, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
