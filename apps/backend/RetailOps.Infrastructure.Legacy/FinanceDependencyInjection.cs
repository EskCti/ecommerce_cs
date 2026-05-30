using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.Catalog.Application.Events;
using RetailOps.Core.Finance.Application.Ports;
using RetailOps.Core.Finance.Application.Queries;
using RetailOps.Core.Finance.Application.UseCases;
using RetailOps.Core.Finance.Domain.Repositories;
using RetailOps.Core.Finance.Domain.Services;
using RetailOps.Core.Sales.Application.Events;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Infrastructure.Legacy;

public static class FinanceDependencyInjection
{
    public static IServiceCollection AddFinanceModule(this IServiceCollection services)
    {
        services.AddScoped<Finance.FinanceLegacyAdapter>();
        services.AddScoped<IFinanceLegacyPort>(sp => sp.GetRequiredService<Finance.FinanceLegacyAdapter>());

        services.AddScoped<IReceivableRepository, Finance.LegacyReceivableRepository>();
        services.AddScoped<IPayableRepository, Finance.LegacyPayableRepository>();
        services.AddScoped<ICommissionRepository, Finance.LegacyCommissionRepository>();
        services.AddScoped<IFinanceQueries, Finance.FinanceQueries>();
        services.AddScoped<IFinanceCancellationPort, FinanceCancellationService>();

        services.AddScoped<ReceivableSettlementService>();
        services.AddScoped<PayableSettlementService>();
        services.AddScoped<CommissionSettlementService>();
        services.AddScoped<CashFlowAggregator>();

        services.AddScoped<CreateReceivableUseCase>();
        services.AddScoped<UpdateReceivableUseCase>();
        services.AddScoped<DeleteReceivableUseCase>();
        services.AddScoped<GetReceivableUseCase>();
        services.AddScoped<SettleReceivableUseCase>();
        services.AddScoped<AddReceivableAttachmentUseCase>();

        services.AddScoped<CreatePayableUseCase>();
        services.AddScoped<UpdatePayableUseCase>();
        services.AddScoped<DeletePayableUseCase>();
        services.AddScoped<GetPayableUseCase>();
        services.AddScoped<SettlePayableUseCase>();
        services.AddScoped<AddPayableAttachmentUseCase>();

        services.AddScoped<PayCommissionUseCase>();
        services.AddScoped<PayCommissionsBatchUseCase>();

        services.AddScoped<SaleCompletedHandler>();
        services.AddScoped<ProductPurchasedHandler>();

        services.AddScoped<ISaleCompletedPublisher, FinanceSaleCompletedPublisher>();
        services.AddScoped<IProductPurchasedPublisher, FinanceProductPurchasedPublisher>();

        return services;
    }
}

internal sealed class FinanceSaleCompletedPublisher(SaleCompletedHandler handler) : ISaleCompletedPublisher
{
    public async Task<Result> Publish(SaleCompletedEvent saleEvent, CancellationToken ct = default) =>
        await handler.Handle(saleEvent);
}

internal sealed class FinanceProductPurchasedPublisher(ProductPurchasedHandler handler) : IProductPurchasedPublisher
{
    public async Task PublishAsync(ProductPurchased @event, CancellationToken ct = default)
    {
        var result = await handler.Handle(@event);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
    }
}
