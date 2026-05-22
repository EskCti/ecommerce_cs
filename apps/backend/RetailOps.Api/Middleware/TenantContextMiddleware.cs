using System.Security.Claims;
using RetailOps.Infrastructure.MultiTenancy;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Api.Middleware;

public sealed class TenantContextMiddleware
{
    private const string TenantHeader = "X-Tenant-Id";
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantAccessor)
    {
        var tenantId = ResolveTenantId(context);
        tenantAccessor.SetTenant(tenantId);
        await _next(context);
    }

    private static TenantId ResolveTenantId(HttpContext context)
    {
        var claim = context.User.FindFirstValue("tenant_id")
            ?? context.User.FindFirstValue("empresa");

        if (int.TryParse(claim, out var fromClaim))
        {
            var claimResult = TenantId.Create(fromClaim);
            if (claimResult.IsSuccess)
                return claimResult.Value;
        }

        if (context.Request.Headers.TryGetValue(TenantHeader, out var header)
            && int.TryParse(header.FirstOrDefault(), out var fromHeader))
        {
            var result = TenantId.Create(fromHeader);
            if (result.IsSuccess)
                return result.Value;
        }

        return TenantId.Platform;
    }
}
