using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Application.Queries;
using RetailOps.Core.Crm.Application.UseCases;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/crm/suppliers")]
[Authorize]
public sealed class CrmSuppliersController(
    ITenantContext tenantContext,
    CreateSupplierUseCase createSupplierUseCase,
    UpdateSupplierUseCase updateSupplierUseCase,
    DeactivateSupplierUseCase deactivateSupplierUseCase,
    GetSupplierUseCase getSupplierUseCase,
    ISupplierQueries supplierQueries) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet]
    public async Task<IActionResult> ListSuppliers(
        [FromQuery] string? name,
        [FromQuery] PersonType? personType,
        [FromQuery] string? taxDocument,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new SupplierListFilter(name, personType, taxDocument, isActive, page, pageSize);
        var result = await supplierQueries.ListByTenantAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSupplier(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await getSupplierUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await createSupplierUseCase.Execute((TenantId, request), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetSupplier), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(
        Guid id,
        [FromBody] UpdateSupplierInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updateSupplierUseCase.Execute((TenantId, id, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateSupplier(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await deactivateSupplierUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
