using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.StoreSettings.Application.Queries;
using RetailOps.Core.StoreSettings.Application.UseCases;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Infrastructure.Legacy.StoreSettings;

namespace RetailOps.Infrastructure.Legacy;

public static class StoreSettingsDependencyInjection
{
    public static IServiceCollection AddStoreSettingsModule(this IServiceCollection services)
    {
        services.AddScoped<IStoreConfigRepository, LegacyStoreConfigRepository>();
        services.AddScoped<IPaymentMethodRepository, LegacyPaymentMethodRepository>();
        services.AddScoped<ICashRegisterTerminalRepository, LegacyCashRegisterTerminalRepository>();
        services.AddScoped<IPaymentMethodQueries, PaymentMethodQueries>();
        services.AddScoped<ICashRegisterTerminalQueries, CashRegisterTerminalQueries>();

        services.AddScoped<GetStoreConfigUseCase>();
        services.AddScoped<UpdateStoreConfigUseCase>();
        services.AddScoped<CreatePaymentMethodUseCase>();
        services.AddScoped<UpdatePaymentMethodUseCase>();
        services.AddScoped<DeletePaymentMethodUseCase>();
        services.AddScoped<GetPaymentMethodUseCase>();
        services.AddScoped<ListPaymentMethodsUseCase>();
        services.AddScoped<CreateCashRegisterTerminalUseCase>();
        services.AddScoped<UpdateCashRegisterTerminalUseCase>();
        services.AddScoped<DeleteCashRegisterTerminalUseCase>();
        services.AddScoped<GetCashRegisterTerminalUseCase>();
        services.AddScoped<ListCashRegisterTerminalsUseCase>();

        return services;
    }
}
