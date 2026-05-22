using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Application.Queries;
using RetailOps.Identity.Core.Application.UseCases;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class UsersPermissionsController(
    ListUsersByTenantQuery listUsers,
    AssignPermissionsUseCase assignPermissions,
    IPermissionCatalogQuery catalog,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("users")]
    [Authorize(Policy = "Permission:usuarios")]
    public async Task<IActionResult> ListUsers(CancellationToken ct)
    {
        var tenant = tenantContext.TenantId.Value;
        var result = await listUsers.Execute(tenant, ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("users/{userId:guid}/permissions")]
    [Authorize(Policy = "Permission:usuarios")]
    public async Task<IActionResult> AssignPermissions(Guid userId, [FromBody] AssignPermissionsBody body, CancellationToken ct)
    {
        var result = await assignPermissions.Execute(new AssignPermissionsInDto(userId, body.PermissionKeys));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpGet("permissions/catalog")]
    public async Task<IActionResult> PermissionCatalog(CancellationToken ct) =>
        Ok(await catalog.ListAsync(ct));

    public sealed record AssignPermissionsBody(IReadOnlyList<string> PermissionKeys);
}
