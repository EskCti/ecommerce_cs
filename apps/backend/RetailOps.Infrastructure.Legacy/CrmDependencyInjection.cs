using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.Crm.Application.Queries;
using RetailOps.Core.Crm.Application.UseCases;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.Services;
using RetailOps.Infrastructure.Legacy.Crm;

namespace RetailOps.Infrastructure.Legacy;

public static class CrmDependencyInjection
{
    public static IServiceCollection AddCrmModule(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, LegacyCustomerRepository>();
        services.AddScoped<ISupplierRepository, LegacySupplierRepository>();
        services.AddScoped<ICustomerQueries, CustomerQueries>();
        services.AddScoped<ISupplierQueries, SupplierQueries>();
        services.AddScoped<CustomerRegistrationPolicy>();

        services.AddScoped<CreateCustomerUseCase>();
        services.AddScoped<UpdateCustomerUseCase>();
        services.AddScoped<DeactivateCustomerUseCase>();
        services.AddScoped<GetCustomerUseCase>();
        services.AddScoped<FindOrCreateByCpfUseCase>();
        services.AddScoped<AddCustomerAttachmentUseCase>();
        services.AddScoped<CreateSupplierUseCase>();
        services.AddScoped<UpdateSupplierUseCase>();
        services.AddScoped<DeactivateSupplierUseCase>();
        services.AddScoped<GetSupplierUseCase>();

        return services;
    }
}
