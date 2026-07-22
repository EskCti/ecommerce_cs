using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.Reporting.Application.Ports;
using RetailOps.Core.Reporting.Application.Queries;

namespace RetailOps.Infrastructure.Legacy;

public static class ReportingDependencyInjection
{
    public static IServiceCollection AddReportingModule(this IServiceCollection services)
    {
        services.AddScoped<Reporting.LegacyReportSqlAdapter>();
        services.AddScoped<IReportingLegacyPort>(sp => sp.GetRequiredService<Reporting.LegacyReportSqlAdapter>());
        services.AddScoped<IReportingStoreBrandingPort, Reporting.ReportingStoreBrandingAdapter>();
        services.AddScoped<IPdfRendererPort, Reporting.QuestPdfRendererAdapter>();

        services.AddScoped<IReportSalesQuery, ReportSalesQuery>();
        services.AddScoped<IReportLowStockQuery, ReportLowStockQuery>();
        services.AddScoped<IReportCashSessionsQuery, ReportCashSessionsQuery>();
        services.AddScoped<IReportProfitQuery, ReportProfitQuery>();
        services.AddScoped<IGenerateReceiptQuery, GenerateReceiptQuery>();

        return services;
    }
}
