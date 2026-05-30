using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Finance.Application.DTOs;
using RetailOps.Core.Finance.Application.Queries;
using RetailOps.Core.Finance.Application.UseCases;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/finance/receivables")]
[Authorize]
public sealed class FinanceReceivablesController(
    ITenantContext tenantContext,
    CreateReceivableUseCase createReceivableUseCase,
    UpdateReceivableUseCase updateReceivableUseCase,
    DeleteReceivableUseCase deleteReceivableUseCase,
    GetReceivableUseCase getReceivableUseCase,
    SettleReceivableUseCase settleReceivableUseCase,
    AddReceivableAttachmentUseCase addReceivableAttachmentUseCase,
    IFinanceQueries financeQueries) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] PaymentStatus? status,
        [FromQuery] DateTime? dueFrom,
        [FromQuery] DateTime? dueTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new ReceivableListFilter(status, dueFrom, dueTo, page, pageSize);
        var result = await financeQueries.ListReceivablesAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await getReceivableUseCase.Execute((TenantId, id));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReceivableInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await createReceivableUseCase.Execute((TenantId, request));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReceivableInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updateReceivableUseCase.Execute((TenantId, id, request));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await deleteReceivableUseCase.Execute((TenantId, id));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : NoContent();
    }

    [HttpPost("{id:guid}/settle")]
    public async Task<IActionResult> Settle(Guid id, [FromBody] SettleAccountInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await settleReceivableUseCase.Execute((TenantId, id, request));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("{id:guid}/attachments")]
    public async Task<IActionResult> AddAttachment(
        Guid id,
        [FromBody] AddFinanceAttachmentInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await addReceivableAttachmentUseCase.Execute((TenantId, id, request));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
