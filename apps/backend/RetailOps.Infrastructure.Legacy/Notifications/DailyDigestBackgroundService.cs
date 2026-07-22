using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RetailOps.Core.Notifications.Application.Ports;
using RetailOps.Core.Notifications.Application.UseCases;

namespace RetailOps.Infrastructure.Legacy.Notifications;

public sealed class DailyDigestBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<WhatsAppSettings> settings,
    ILogger<DailyDigestBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            logger.LogInformation("Daily digest scheduled in {Delay}", delay);
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await RunDigestForAllTenantsAsync(stoppingToken);
        }
    }

    internal static TimeSpan GetDelayUntilNextRun(DateTime utcNow, WhatsAppSettings settings)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.ScheduleTimeZone);
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        var nextRunLocal = new DateTime(
            localNow.Year,
            localNow.Month,
            localNow.Day,
            settings.ScheduleHour,
            0,
            0,
            DateTimeKind.Unspecified);

        if (localNow >= nextRunLocal)
            nextRunLocal = nextRunLocal.AddDays(1);

        var nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(nextRunLocal, timeZone);
        return nextRunUtc - utcNow;
    }

    private TimeSpan GetDelayUntilNextRun() =>
        GetDelayUntilNextRun(DateTime.UtcNow, settings.Value);

    private async Task RunDigestForAllTenantsAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var tenantsPort = scope.ServiceProvider.GetRequiredService<IActiveTenantIdsQueryPort>();
        var handler = scope.ServiceProvider.GetRequiredService<SendDailyTenantDigestHandler>();

        var tenantsResult = await tenantsPort.ListActiveTenantIdsAsync(ct);
        if (tenantsResult.IsFailure)
        {
            logger.LogWarning("Failed to list active tenants: {Error}", tenantsResult.Error);
            return;
        }

        foreach (var tenantId in tenantsResult.Value)
        {
            var result = await handler.ExecuteAsync(tenantId, ct);
            if (result.IsFailure)
                logger.LogWarning("Digest failed for tenant {TenantId}: {Error}", tenantId.Value, result.Error);
            else
                logger.LogInformation("Digest result for tenant {TenantId}: {Status}", tenantId.Value, result.Value);
        }
    }
}
