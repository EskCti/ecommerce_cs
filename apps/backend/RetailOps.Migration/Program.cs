using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RetailOps.Core.Cutover;
using RetailOps.Infrastructure;
using RetailOps.Infrastructure.Cutover;
using RetailOps.Infrastructure.Legacy;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddInfrastructure(context.Configuration);
        services.AddCutoverModule();
        services.AddLegacyInfrastructure(context.Configuration);
        services.AddCutoverLegacyServices();
    })
    .Build();

await CutoverDatabaseBootstrap.EnsureSchemaAsync(host.Services);

var tenantArg = args.FirstOrDefault(a => a.StartsWith("--tenant=", StringComparison.OrdinalIgnoreCase));
int? tenantId = tenantArg is not null && int.TryParse(tenantArg["--tenant=".Length..], out var parsed)
    ? parsed
    : null;

var reconcileOnly = args.Any(a => string.Equals(a, "--reconcile", StringComparison.OrdinalIgnoreCase));

await using var scope = host.Services.CreateAsyncScope();
var sp = scope.ServiceProvider;

if (reconcileOnly)
{
    if (!tenantId.HasValue)
    {
        Console.Error.WriteLine("--reconcile requires --tenant=<id>");
        return 1;
    }

    var reconciliation = sp.GetRequiredService<IReconciliationReportService>();
    var report = await reconciliation.BuildAsync(tenantId.Value);
    Console.WriteLine($"tenant={report.TenantId} critical={report.HasCriticalDiscrepancies}");
    foreach (var line in report.Lines)
    {
        Console.WriteLine(
            $"{line.BoundedContext}/{line.Aggregate}: legacy={line.LegacyCount} normalized={line.NormalizedCount} match={line.LegacyCount == line.NormalizedCount}");
    }

    return report.HasCriticalDiscrepancies ? 2 : 0;
}

var job = sp.GetRequiredService<IHistoricalDataMigrationJob>();
IReadOnlyList<int>? tenantIds = tenantId.HasValue ? [tenantId.Value] : null;
await job.RunAsync(tenantIds);
Console.WriteLine("HistoricalDataMigrationJob completed.");
return 0;
