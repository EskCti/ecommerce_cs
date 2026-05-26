using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.StoreSettings.Application.Ports;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Transactions;

namespace RetailOps.Infrastructure.Legacy;

public static class DependencyInjection
{
    public static IServiceCollection AddLegacyInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<LegacySasDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITransactionManager, LegacyEfTransactionManager>();

        // Store Settings Legacy Adapter
        services.AddScoped<IStoreSettingsLegacyPort, StoreSettingsLegacyAdapter>();

        return services;
    }
}