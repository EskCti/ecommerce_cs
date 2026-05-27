using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Application.UseCases;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/catalog/products")]
[Authorize]
public sealed class CatalogProductsController(
    ITenantContext tenantContext,
    CreateProductUseCase createProductUseCase,
    UpdateProductUseCase updateProductUseCase,
    DeactivateProductUseCase deactivateProductUseCase,
    GetProductUseCase getProductUseCase,
    GenerateBarcodeUseCase generateBarcodeUseCase,
    IProductCatalogService productCatalogService,
    IProductQueries productQueries) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet]
    public async Task<IActionResult> ListProducts(
        [FromQuery] string? barcode,
        [FromQuery] string? name,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new ProductListFilter(barcode, name, categoryId, isActive, page, pageSize);
        var result = await productQueries.ListByTenantAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await getProductUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpGet("by-barcode/{code}")]
    public async Task<IActionResult> GetByBarcode(string code, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await productCatalogService.FindByBarcodeAsync(tenantContext.TenantId, code, ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        if (result.Value is null)
            return NotFound();

        return Ok(ProductOutputDto.FromDomain(result.Value));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductInputDto request, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await createProductUseCase.Execute((TenantId, request), ct);
        if (result.IsFailure)
        {
            if (result.Error.StartsWith("DUPLICATE_BARCODE", StringComparison.Ordinal))
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetProduct), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await updateProductUseCase.Execute((TenantId, id, request), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateProduct(Guid id, CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await deactivateProductUseCase.Execute((TenantId, id), ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("generate-barcode")]
    public async Task<IActionResult> GenerateBarcode(CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await generateBarcodeUseCase.Execute(TenantId, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
