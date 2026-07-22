using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.Cutover;
using RetailOps.Core.Sales.Application.Ports;

namespace RetailOps.Infrastructure.Cutover;

public static class CutoverDependencyInjection
{
    public static IServiceCollection AddCutoverModule(this IServiceCollection services)
    {
        services.AddScoped<IMigrationCheckpointStore, MigrationCheckpointStore>();
        services.AddScoped<IParallelRunStatsRecorder, ParallelRunStatsRecorder>();
        services.AddScoped<ICutoverGateService, CutoverGateService>();
        services.AddScoped<IHistoricalDataMigrationJob, HistoricalDataMigrationJob>();
        services.AddScoped<IReconciliationReportService, ReconciliationReportService>();
        return services;
    }

    public static IServiceCollection AddSalesParallelRunLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dualWriteEnabled = configuration.GetSection("Migration").GetValue("DualWriteEnabled", true);
        if (dualWriteEnabled)
            services.AddScoped<ISaleParallelRunLogger, StatsRecordingSaleParallelRunLogger>();
        else
            services.AddScoped<ISaleParallelRunLogger, NullSaleParallelRunLogger>();

        return services;
    }
}
