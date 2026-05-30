using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Finance.Application.DTOs;
using RetailOps.Core.Finance.Application.Queries;
using RetailOps.Core.Finance.Application.UseCases;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/finance/payables")]
[Authorize]
public sealed class FinancePayablesController(
    ITenantContext tenantContext,
    CreatePayableUseCase createPayableUseCase,
    UpdatePayableUseCase updatePayableUseCase,
    DeletePayableUseCase deletePayableUseCase,
    GetPayableUseCase getPayableUseCase,
    SettlePayableUseCase settlePayableUseCase,
    AddPayableAttachmentUseCase addPayableAttachmentUseCase,
    IFinanceQueries financeQueries) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] AccountType? type,
        [FromQuery] PaymentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new PayableListFilter(type ?? AccountType.Expense, status, page, pageSize);
        var result = await financeQueries.ListPayablesAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await getPayableUseCase.Execute((TenantId, id));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePayableInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await createPayableUseCase.Execute((TenantId, request));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePayableInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updatePayableUseCase.Execute((TenantId, id, request));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await deletePayableUseCase.Execute((TenantId, id));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : NoContent();
    }

    [HttpPost("{id:guid}/settle")]
    public async Task<IActionResult> Settle(Guid id, [FromBody] SettleAccountInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await settlePayableUseCase.Execute((TenantId, id, request));
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

        var result = await addPayableAttachmentUseCase.Execute((TenantId, id, request));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}

[ApiController]
[Route("api/finance/purchases")]
[Authorize]
public sealed class FinancePurchasesController(
    ITenantContext tenantContext,
    IFinanceQueries financeQueries) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] PaymentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new PayableListFilter(AccountType.Purchase, status, page, pageSize);
        var result = await financeQueries.ListPurchasesAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
