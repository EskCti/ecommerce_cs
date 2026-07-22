using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Notifications.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(SendDailyTenantDigestHandler sendDailyTenantDigestHandler) : ControllerBase
{
    [HttpPost("digest/trigger")]
    public async Task<IActionResult> TriggerDigest([FromQuery] int tenantId, CancellationToken ct)
    {
        if (!IsAdministrator())
            return Forbid();

        var tenantResult = TenantId.Create(tenantId);
        if (tenantResult.IsFailure)
            return BadRequest(new { error = tenantResult.Error });

        var result = await sendDailyTenantDigestHandler.ExecuteAsync(tenantResult.Value, ct);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new
        {
            status = result.Value,
            eventPublished = sendDailyTenantDigestHandler.LastEvent is not null,
        });
    }

    private bool IsAdministrator()
    {
        var level = User.FindFirstValue("user_level");
        return string.Equals(level, "Administrador", StringComparison.OrdinalIgnoreCase)
               || string.Equals(level, "Sas", StringComparison.OrdinalIgnoreCase);
    }
}
