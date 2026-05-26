using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Application.Queries;
using RetailOps.Core.Crm.Application.UseCases;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/crm/customers")]
[Authorize]
public sealed class CrmCustomersController(
    ITenantContext tenantContext,
    CreateCustomerUseCase createCustomerUseCase,
    UpdateCustomerUseCase updateCustomerUseCase,
    DeactivateCustomerUseCase deactivateCustomerUseCase,
    GetCustomerUseCase getCustomerUseCase,
    FindOrCreateByCpfUseCase findOrCreateByCpfUseCase,
    AddCustomerAttachmentUseCase addCustomerAttachmentUseCase,
    ICustomerQueries customerQueries) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet]
    public async Task<IActionResult> ListCustomers(
        [FromQuery] string? name,
        [FromQuery] string? cpf,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new CustomerListFilter(name, cpf, isActive, page, pageSize);
        var result = await customerQueries.ListByTenantAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomer(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await getCustomerUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await createCustomerUseCase.Execute((TenantId, request), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetCustomer), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updateCustomerUseCase.Execute((TenantId, id, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateCustomer(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await deactivateCustomerUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("find-or-create-by-cpf")]
    public async Task<IActionResult> FindOrCreateByCpf(
        [FromBody] FindOrCreateByCpfInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await findOrCreateByCpfUseCase.Execute((TenantId, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("{id:guid}/attachments")]
    public async Task<IActionResult> ListAttachments(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await customerQueries.ListAttachmentsAsync(tenantContext.TenantId, id, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("{id:guid}/attachments")]
    public async Task<IActionResult> AddAttachment(
        Guid id,
        [FromBody] AddCustomerAttachmentInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await addCustomerAttachmentUseCase.Execute((TenantId, id, request), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(ListAttachments), new { id }, result.Value);
    }
}
