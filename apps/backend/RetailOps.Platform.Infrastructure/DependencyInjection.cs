using Microsoft.Extensions.DependencyInjection;
using RetailOps.Platform.Core.Application.Queries;
using RetailOps.Platform.Core.Application.Ports;
using RetailOps.Platform.Core.Application.UseCases;
using RetailOps.Platform.Infrastructure.Legacy;

namespace RetailOps.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformModule(this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, LegacyCompanyRepository>();
        services.AddScoped<IPlatformConfigRepository, LegacyPlatformConfigRepository>();
        services.AddScoped<ITenantInvoicePort, LegacyTenantInvoiceAdapter>();
        services.AddScoped<DeactivateUsersOnTenantSuspendedHandler>();
        services.AddScoped<ITenantSuspendedPublisher, InProcessTenantSuspendedPublisher>();

        services.AddScoped<RegisterTrialCompanyUseCase>();
        services.AddScoped<CreateCompanyUseCase>();
        services.AddScoped<UpdateCompanyUseCase>();
        services.AddScoped<SaveContractUseCase>();
        services.AddScoped<IssueTenantInvoiceUseCase>();
        services.AddScoped<SuspendOverdueTenantUseCase>();
        services.AddScoped<ListCompaniesQuery>();
        services.AddScoped<GetCompanyByIdQuery>();

        return services;
    }
}
