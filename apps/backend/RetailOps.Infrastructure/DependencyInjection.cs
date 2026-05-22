using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.Migration;
using RetailOps.Core.MultiTenancy;
using RetailOps.Infrastructure.Migration;
using RetailOps.Infrastructure.MultiTenancy;
using RetailOps.Infrastructure.Persistence.Contexts;
using RetailOps.Infrastructure.Persistence.Transactions;
using RetailOps.Shared.Kernel.Domain.Transactions;

namespace RetailOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<RetailOpsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITransactionManager, EfTransactionManager>();
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContextAccessor>(sp => sp.GetRequiredService<TenantContext>());
        services.Configure<MigrationOptions>(configuration.GetSection("Migration"));
        services.AddSingleton<IMigrationSettings, MigrationSettings>();

        return services;
    }
}
