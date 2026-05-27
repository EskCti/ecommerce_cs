using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Application.UseCases;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/catalog/categories")]
[Authorize]
public sealed class CatalogCategoriesController(
    ITenantContext tenantContext,
    CreateCategoryUseCase createCategoryUseCase,
    UpdateCategoryUseCase updateCategoryUseCase,
    GetCategoryUseCase getCategoryUseCase,
    ICategoryQueries categoryQueries) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet]
    public async Task<IActionResult> ListCategories(
        [FromQuery] string? name,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new CategoryListFilter(name, isActive, page, pageSize);
        var result = await categoryQueries.ListByTenantAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await getCategoryUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await createCategoryUseCase.Execute((TenantId, request), ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetCategory), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updateCategoryUseCase.Execute((TenantId, id, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateCategory(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updateCategoryUseCase.Execute(
            (TenantId, id, new UpdateCategoryInputDto { IsActive = false }),
            ct);

        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
