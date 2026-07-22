using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.Cutover;
using RetailOps.Infrastructure.Legacy.Cutover;

namespace RetailOps.Infrastructure.Legacy;

public static class CutoverLegacyDependencyInjection
{
    public static IServiceCollection AddCutoverLegacyServices(this IServiceCollection services)
    {
        services.AddScoped<ILegacyDataCounter, LegacyDataCounter>();
        return services;
    }
}
