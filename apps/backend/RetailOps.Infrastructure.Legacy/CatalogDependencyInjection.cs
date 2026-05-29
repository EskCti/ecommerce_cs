using Microsoft.Extensions.DependencyInjection;
using RetailOps.Core.Catalog.Application.Events;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Application.UseCases;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Infrastructure.Legacy.Catalog;

namespace RetailOps.Infrastructure.Legacy;

public static class CatalogDependencyInjection
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, LegacyProductRepository>();
        services.AddScoped<ICategoryRepository, LegacyCategoryRepository>();
        services.AddScoped<IStockMovementRepository, LegacyStockMovementRepository>();
        services.AddScoped<IProductQueries, ProductQueries>();
        services.AddScoped<ICategoryQueries, CategoryQueries>();
        services.AddScoped<IListLowStockProductsQuery, LowStockQueries>();

        services.AddScoped<ProductRegistrationPolicy>();
        services.AddScoped<StockAdjustmentPolicy>();
        services.AddScoped<GradeVariantSyncService>();

        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<IProductPurchasedPublisher, ProductPurchasedPublisherStub>();

        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<UpdateProductUseCase>();
        services.AddScoped<DeactivateProductUseCase>();
        services.AddScoped<GetProductUseCase>();
        services.AddScoped<GenerateBarcodeUseCase>();
        services.AddScoped<CreateCategoryUseCase>();
        services.AddScoped<UpdateCategoryUseCase>();
        services.AddScoped<GetCategoryUseCase>();
        services.AddScoped<ConfigureProductGradeUseCase>();
        services.AddScoped<ResolveGradeVariantUseCase>();
        services.AddScoped<RecordStockEntryUseCase>();
        services.AddScoped<RecordStockExitUseCase>();
        services.AddScoped<PurchaseStockUseCase>();

        return services;
    }
}
