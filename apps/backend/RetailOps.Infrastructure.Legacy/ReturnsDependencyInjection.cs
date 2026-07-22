using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.Returns.Application.Ports;
using RetailOps.Core.Returns.Application.Queries;
using RetailOps.Core.Returns.Application.UseCases;
using RetailOps.Core.Returns.Domain.Repositories;
using RetailOps.Core.Returns.Domain.Services;

namespace RetailOps.Infrastructure.Legacy;

public static class ReturnsDependencyInjection
{
    public static IServiceCollection AddReturnsModule(this IServiceCollection services)
    {
        services.AddScoped<Returns.LegacyExchangeRepository>();
        services.AddScoped<IExchangeRepository>(sp => sp.GetRequiredService<Returns.LegacyExchangeRepository>());
        services.AddScoped<IReturnsLegacyPort>(sp => sp.GetRequiredService<Returns.LegacyExchangeRepository>());
        services.AddScoped<IListExchangesQuery, Returns.ReturnsQueries>();
        services.AddScoped<ICustomerProvisioningPort, Returns.CustomerProvisioningAdapter>();

        services.AddScoped<ExchangeStockPolicy>();
        services.AddScoped<RegisterExchangeUseCase>();
        services.AddScoped<DeleteExchangeUseCase>();

        return services;
    }
}
