using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Cutover;
using RetailOps.Core.Migration;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class MigrationStatusController : ControllerBase
{
    private readonly IMigrationSettings _migrationSettings;
    private readonly ILegacyDataCounter _legacyDataCounter;
    private readonly ITenantContext _tenantContext;

    public MigrationStatusController(
        IMigrationSettings migrationSettings,
        ILegacyDataCounter legacyDataCounter,
        ITenantContext tenantContext)
    {
        _migrationSettings = migrationSettings;
        _legacyDataCounter = legacyDataCounter;
        _tenantContext = tenantContext;
    }

    [HttpGet("migration-status")]
    public async Task<IActionResult> GetMigrationStatus(CancellationToken ct)
    {
        if (!_tenantContext.TenantId.IsPlatform)
            return Forbid();

        var activeTenants = await _legacyDataCounter.ListActiveTenantIdsAsync(ct);
        var tenantsOnPhpPath = _migrationSettings.CountTenantsOnPhpPath(activeTenants);

        return Ok(new
        {
            pilotTenantIds = _migrationSettings.PilotTenantIds,
            fullyMigratedTenantIds = _migrationSettings.FullyMigratedTenantIds,
            legacyPhpEnabled = _migrationSettings.LegacyPhpEnabled,
            allTenantsOnRetailOps = _migrationSettings.AllTenantsOnRetailOps,
            dualWriteEnabled = _migrationSettings.DualWriteEnabled,
            boundedContexts = _migrationSettings.BoundedContextFlags,
            activeTenantCount = activeTenants.Count,
            tenantsOnPhpPath,
            cutoverComplete = tenantsOnPhpPath == 0
                && !_migrationSettings.LegacyPhpEnabled
                && _migrationSettings.AllTenantsOnRetailOps
        });
    }
}
