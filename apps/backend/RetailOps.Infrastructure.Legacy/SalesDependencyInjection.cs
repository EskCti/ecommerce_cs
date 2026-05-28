using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Application.Queries;
using RetailOps.Core.Sales.Application.UseCases;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Core.Sales.Domain.Services;
using RetailOps.Infrastructure.Legacy.Sales;

namespace RetailOps.Infrastructure.Legacy;

public static class SalesDependencyInjection
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services)
    {
        services.AddScoped<ICashSessionRepository, LegacyCashSessionRepository>();
        services.AddScoped<ISaleRepository, LegacySaleRepository>();
        services.AddScoped<IListSalesQuery, SalesQueries>();

        services.AddScoped<CartStockReservationService>();

        services.AddScoped<IManagerPinVerifier, ManagerPinVerifierStub>();
        services.AddScoped<ISaleParallelRunLogger, SaleParallelRunLogger>();
        services.AddScoped<ISaleCompletedPublisher, SaleCompletedPublisherStub>();

        services.AddScoped<OpenCashSessionUseCase>();
        services.AddScoped<RegisterCashWithdrawalUseCase>();
        services.AddScoped<CloseCashSessionUseCase>();
        services.AddScoped<AddItemToCartUseCase>();
        services.AddScoped<ConfirmGradeForItemUseCase>();
        services.AddScoped<RemoveCartLineUseCase>();
        services.AddScoped<FinalizeSaleUseCase>();
        services.AddScoped<CancelSaleUseCase>();
        services.AddScoped<GetCurrentCashSessionUseCase>();

        return services;
    }
}
