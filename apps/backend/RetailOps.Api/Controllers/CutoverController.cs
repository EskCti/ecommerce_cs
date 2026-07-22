using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Cutover;
using RetailOps.Core.Migration;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class CutoverController : ControllerBase
{
    private readonly ICutoverGateService _cutoverGate;
    private readonly IHistoricalDataMigrationJob _etlJob;
    private readonly IReconciliationReportService _reconciliation;
    private readonly ILegacyDataCounter _legacyDataCounter;
    private readonly IMigrationSettings _migrationSettings;
    private readonly ITenantContext _tenantContext;

    public CutoverController(
        ICutoverGateService cutoverGate,
        IHistoricalDataMigrationJob etlJob,
        IReconciliationReportService reconciliation,
        ILegacyDataCounter legacyDataCounter,
        IMigrationSettings migrationSettings,
        ITenantContext tenantContext)
    {
        _cutoverGate = cutoverGate;
        _etlJob = etlJob;
        _reconciliation = reconciliation;
        _legacyDataCounter = legacyDataCounter;
        _migrationSettings = migrationSettings;
        _tenantContext = tenantContext;
    }

    [HttpGet("cutover-checklist")]
    public async Task<IActionResult> GetCutoverChecklist(CancellationToken ct)
    {
        if (!IsPlatformAdmin())
            return Forbid();

        var result = await _cutoverGate.RunPreCutoverChecklistAsync(ct);
        return Ok(new
        {
            canProceed = result.CanProceed,
            items = result.Items.Select(i => new { i.Name, i.Passed, i.Detail })
        });
    }

    [HttpPost("migration/etl/run")]
    public async Task<IActionResult> RunHistoricalEtl([FromQuery] int? tenantId, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
            return Forbid();

        IReadOnlyList<int>? tenantIds = tenantId.HasValue ? [tenantId.Value] : null;
        await _etlJob.RunAsync(tenantIds, ct);
        return Accepted(new { status = "completed", tenantId });
    }

    [HttpGet("migration/reconcile")]
    public async Task<IActionResult> GetReconciliationReport([FromQuery] int tenantId, CancellationToken ct)
    {
        if (!IsPlatformAdmin())
            return Forbid();

        var report = await _reconciliation.BuildAsync(tenantId, ct);
        return Ok(new
        {
            tenantId = report.TenantId,
            generatedAtUtc = report.GeneratedAtUtc,
            hasCriticalDiscrepancies = report.HasCriticalDiscrepancies,
            lines = report.Lines.Select(l => new
            {
                l.BoundedContext,
                l.Aggregate,
                l.LegacyCount,
                l.NormalizedCount,
                l.SampleChecksumLegacy,
                l.SampleChecksumNormalized,
                l.IsCritical,
                match = l.LegacyCount == l.NormalizedCount
                    && l.SampleChecksumLegacy == l.SampleChecksumNormalized
            })
        });
    }

    private bool IsPlatformAdmin() => _tenantContext.TenantId.IsPlatform;
}
