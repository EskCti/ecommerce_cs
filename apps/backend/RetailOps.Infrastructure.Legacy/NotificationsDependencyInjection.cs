using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RetailOps.Core.Notifications.Application.Ports;
using RetailOps.Core.Notifications.Application.UseCases;
using RetailOps.Core.Notifications.Domain.Repositories;
using RetailOps.Core.Notifications.Domain.Services;

namespace RetailOps.Infrastructure.Legacy;

public static class NotificationsDependencyInjection
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.Configure<Notifications.WhatsAppSettings>(
            configuration.GetSection(Notifications.WhatsAppSettings.SectionName));

        services.AddHttpClient<Notifications.EnviameWhatsAppAdapter>();

        services.AddScoped<Notifications.LegacyDigestQueryAdapter>();
        services.AddScoped<IReceivablesDueTodayQueryPort>(sp =>
            sp.GetRequiredService<Notifications.LegacyDigestQueryAdapter>());
        services.AddScoped<ILowStockSummaryQueryPort>(sp =>
            sp.GetRequiredService<Notifications.LegacyDigestQueryAdapter>());
        services.AddScoped<ITenantBillingAlertQueryPort>(sp =>
            sp.GetRequiredService<Notifications.LegacyDigestQueryAdapter>());
        services.AddScoped<IActiveTenantIdsQueryPort>(sp =>
            sp.GetRequiredService<Notifications.LegacyDigestQueryAdapter>());

        services.AddScoped<Notifications.LegacyDailyDigestLogRepository>();
        services.AddScoped<IDailyDigestLogRepository>(sp =>
            sp.GetRequiredService<Notifications.LegacyDailyDigestLogRepository>());

        services.AddScoped<IWhatsAppGatewayPort, Notifications.EnviameWhatsAppAdapter>();
        services.AddScoped<IWhatsAppCredentialsResolver, Notifications.WhatsAppCredentialsResolver>();
        services.AddScoped<INotificationStoreNamePort, Notifications.NotificationStoreNameAdapter>();

        services.AddScoped<DailyDigestComposer>();
        services.AddScoped<SendDailyTenantDigestHandler>();

        return services;
    }

    public static IServiceCollection AddNotificationsHostedService(this IServiceCollection services)
    {
        services.AddHostedService<Notifications.DailyDigestBackgroundService>();
        return services;
    }
}
