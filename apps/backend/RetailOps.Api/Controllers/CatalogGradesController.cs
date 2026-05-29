using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.UseCases;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/catalog/products/{productId:guid}/grades")]
[Authorize]
public sealed class CatalogGradesController(
    ITenantContext tenantContext,
    ConfigureProductGradeUseCase configureProductGradeUseCase,
    GetProductUseCase getProductUseCase) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet]
    public async Task<IActionResult> ListGrades(Guid productId, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await getProductUseCase.Execute((TenantId, productId), ct);
        return result.IsFailure
            ? BadRequest(new { error = result.Error })
            : Ok(GradeConfigurationOutputDto.FromProductOutput(result.Value));
    }

    [HttpPost]
    public async Task<IActionResult> ConfigureGrade(
        Guid productId,
        [FromBody] ConfigureProductGradeInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await configureProductGradeUseCase.Execute((TenantId, productId, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
