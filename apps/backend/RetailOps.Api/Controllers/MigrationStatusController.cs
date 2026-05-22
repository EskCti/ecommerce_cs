using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Migration;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class MigrationStatusController : ControllerBase
{
    private readonly IMigrationSettings _migrationSettings;
    private readonly ITenantContext _tenantContext;

    public MigrationStatusController(
        IMigrationSettings migrationSettings,
        ITenantContext tenantContext)
    {
        _migrationSettings = migrationSettings;
        _tenantContext = tenantContext;
    }

    [HttpGet("migration-status")]
    public IActionResult GetMigrationStatus()
    {
        if (!_tenantContext.TenantId.IsPlatform)
            return Forbid();

        return Ok(new
        {
            pilotTenantIds = _migrationSettings.PilotTenantIds,
            boundedContexts = _migrationSettings.BoundedContextFlags
        });
    }
}
