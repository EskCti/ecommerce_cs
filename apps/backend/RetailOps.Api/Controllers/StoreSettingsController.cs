using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.MultiTenancy;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Application.Queries;
using RetailOps.Core.StoreSettings.Application.UseCases;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public sealed class StoreSettingsController(
    ITenantContext tenantContext,
    GetStoreConfigUseCase getStoreConfigUseCase,
    UpdateStoreConfigUseCase updateStoreConfigUseCase,
    CreatePaymentMethodUseCase createPaymentMethodUseCase,
    UpdatePaymentMethodUseCase updatePaymentMethodUseCase,
    DeletePaymentMethodUseCase deletePaymentMethodUseCase,
    CreateCashRegisterTerminalUseCase createCashRegisterTerminalUseCase,
    UpdateCashRegisterTerminalUseCase updateCashRegisterTerminalUseCase,
    DeleteCashRegisterTerminalUseCase deleteCashRegisterTerminalUseCase,
    IPaymentMethodQueries paymentMethodQueries,
    ICashRegisterTerminalQueries cashRegisterTerminalQueries) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet("store-config")]
    public async Task<IActionResult> GetStoreConfig(CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await getStoreConfigUseCase.Execute(TenantId, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPut("store-config")]
    public async Task<IActionResult> UpdateStoreConfig([FromBody] UpdateStoreConfigInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updateStoreConfigUseCase.Execute((TenantId, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("payment-methods")]
    public async Task<IActionResult> ListPaymentMethods(CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await paymentMethodQueries.ListByTenantAsync(tenantContext.TenantId, ct: ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("payment-methods")]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await createPaymentMethodUseCase.Execute((TenantId, request), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(ListPaymentMethods), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("payment-methods/{id:guid}")]
    public async Task<IActionResult> UpdatePaymentMethod(Guid id, [FromBody] UpdatePaymentMethodInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updatePaymentMethodUseCase.Execute((TenantId, id, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpDelete("payment-methods/{id:guid}")]
    public async Task<IActionResult> DeletePaymentMethod(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await deletePaymentMethodUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : NoContent();
    }

    [HttpGet("cash-registers")]
    public async Task<IActionResult> ListCashRegisters(CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await cashRegisterTerminalQueries.ListByTenantAsync(tenantContext.TenantId, ct: ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("cash-registers")]
    public async Task<IActionResult> CreateCashRegister([FromBody] CreateCashRegisterTerminalInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await createCashRegisterTerminalUseCase.Execute((TenantId, request), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(ListCashRegisters), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("cash-registers/{id:guid}")]
    public async Task<IActionResult> UpdateCashRegister(Guid id, [FromBody] UpdateCashRegisterTerminalInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updateCashRegisterTerminalUseCase.Execute((TenantId, id, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpDelete("cash-registers/{id:guid}")]
    public async Task<IActionResult> DeleteCashRegister(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await deleteCashRegisterTerminalUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : NoContent();
    }
}
