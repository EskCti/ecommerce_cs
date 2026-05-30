using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Core.Finance.Application.DTOs;
using RetailOps.Core.Finance.Application.Queries;
using RetailOps.Core.Finance.Application.UseCases;
using RetailOps.Core.MultiTenancy;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/finance/commissions")]
[Authorize]
public sealed class FinanceCommissionsController(
    ITenantContext tenantContext,
    PayCommissionUseCase payCommissionUseCase,
    PayCommissionsBatchUseCase payCommissionsBatchUseCase,
    IFinanceQueries financeQueries) : ControllerBase
{
    private int TenantId => tenantContext.TenantId.Value;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool? isPaid,
        [FromQuery] int? sellerLegacyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var filter = new CommissionListFilter(isPaid, sellerLegacyId, page, pageSize);
        var result = await financeQueries.ListCommissionsAsync(tenantContext.TenantId, filter, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(
        Guid id,
        [FromBody] SettleAccountInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await payCommissionUseCase.Execute((TenantId, id, request));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }

    [HttpPost("pay/batch")]
    public async Task<IActionResult> PayBatch(
        [FromBody] PayCommissionsBatchInputDto request,
        CancellationToken ct)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await payCommissionsBatchUseCase.Execute((TenantId, request));
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}

[ApiController]
[Route("api/finance/cash-flow")]
[Authorize]
public sealed class CashFlowController(
    ITenantContext tenantContext,
    IFinanceQueries financeQueries) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct = default)
    {
        if (!tenantContext.HasTenant)
            return Forbid();

        var result = await financeQueries.GetCashFlowAsync(tenantContext.TenantId, from, to, ct);
        return result.IsFailure ? BadRequest(new { error = result.Error }) : Ok(result.Value);
    }
}
