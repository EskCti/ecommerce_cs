using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Application.Queries;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Sales;
using RetailOps.IntegrationTests.Support;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class SalesEndpointTests : IClassFixture<SalesWebApplicationFactory>
{
    private const string ProductBarcode = "5551234567890";

    private readonly HttpClient _client;
    private readonly SalesWebApplicationFactory _factory;

    public SalesEndpointTests(SalesWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCurrent_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/sales/cash-session/current");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PdvFlow_OpenAddFinalizeClose_WorksForTenant()
    {
        var operatorId = SalesWebApplicationFactory.CreateTenantAdmin(1).Id;
        AuthWebApplicationFactory.ConfigureUser(SalesWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Reset();

        var token = await LoginToken("tenant1@test.com");
        var terminalId = await SeedOpenTerminal();
        var paymentMethodId = await SeedPaymentMethod();
        var categoryId = await CreateCategory(token);
        await CreateProduct(token, categoryId);

        var open = await AuthorizedPost("/api/sales/cash-session/open", token, new
        {
            terminalId,
            managerUserId = operatorId,
            managerPin = "1234",
            openingFloat = 100m,
        });
        Assert.Equal(HttpStatusCode.Created, open.StatusCode);

        var add = await AuthorizedPost("/api/sales/cash-session/current/cart/items", token, new
        {
            scannedValue = ProductBarcode,
        });
        Assert.Equal(HttpStatusCode.OK, add.StatusCode);

        var finalize = await AuthorizedPost("/api/sales/cash-session/finalize", token, new
        {
            paymentMethodId,
            paymentTerms = "Cash",
            amountPaid = 100m,
            discountAmount = 0m,
        });
        Assert.Equal(HttpStatusCode.OK, finalize.StatusCode);

        var close = await AuthorizedPost("/api/sales/cash-session/close", token, new
        {
            managerUserId = operatorId,
            managerPin = "1234",
            countedCash = 180m,
        });
        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
    }

    [Fact]
    public async Task FinalizeCredit_WithoutCustomer_ReturnsBadRequest()
    {
        var operatorId = SalesWebApplicationFactory.CreateTenantAdmin(1).Id;
        AuthWebApplicationFactory.ConfigureUser(SalesWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Reset();

        var token = await LoginToken("tenant1@test.com");
        var terminalId = await SeedOpenTerminal();
        var paymentMethodId = await SeedPaymentMethod();
        var categoryId = await CreateCategory(token);
        await CreateProduct(token, categoryId);

        await AuthorizedPost("/api/sales/cash-session/open", token, new
        {
            terminalId,
            managerUserId = operatorId,
            managerPin = "1234",
            openingFloat = 50m,
        });

        await AuthorizedPost("/api/sales/cash-session/current/cart/items", token, new
        {
            scannedValue = ProductBarcode,
        });

        var finalize = await AuthorizedPost("/api/sales/cash-session/finalize", token, new
        {
            paymentMethodId,
            paymentTerms = "Credit",
            amountPaid = 100m,
        });

        Assert.Equal(HttpStatusCode.BadRequest, finalize.StatusCode);
        var body = await finalize.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("Customer", body.GetProperty("error").GetString());
    }

    private async Task<string> LoginToken(string login)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { login, password = "secret" });
        loginResponse.EnsureSuccessStatusCode();
        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private async Task<Guid> SeedOpenTerminal()
    {
        var tenantId = TenantId.Create(1).Value;
        var terminal = CashRegisterTerminal.Create(
            tenantId,
            CashRegisterTerminalName.Create("PDV 1").Value,
            TerminalStatus.Open).Value;

        await _factory.StoreSettingsFixture.CashRegisterTerminalRepository.Save(terminal);
        return terminal.Id;
    }

    private async Task<Guid> SeedPaymentMethod()
    {
        var tenantId = TenantId.Create(1).Value;
        var method = PaymentMethod.Create(
            tenantId,
            PaymentMethodName.Create("Dinheiro").Value,
            SurchargePercent.Create(0).Value,
            true).Value;

        await _factory.StoreSettingsFixture.PaymentMethodRepository.Save(method);
        return method.Id;
    }

    private async Task<Guid> CreateCategory(string token)
    {
        var response = await AuthorizedPost("/api/catalog/categories", token, new { name = "PDV Cat" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task CreateProduct(string token, Guid categoryId)
    {
        var response = await AuthorizedPost("/api/catalog/products", token, new
        {
            barcode = ProductBarcode,
            name = "Item PDV",
            salePrice = 25m,
            costPrice = 10m,
            initialStock = 50,
            stockAlertLevel = 5,
            categoryId,
        });
        response.EnsureSuccessStatusCode();
    }

    private Task<HttpResponseMessage> AuthorizedPost(string url, string token, object body)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(req);
    }
}

public class SalesWebApplicationFactory : AuthWebApplicationFactory
{
    internal InMemoryCatalogFixture CatalogFixture { get; } = new();
    internal InMemoryStoreSettingsFixture StoreSettingsFixture { get; } = new();
    internal InMemorySalesFixture SalesFixture { get; } = new();

    public void Reset()
    {
        CatalogFixture.Reset();
        StoreSettingsFixture.Reset();
        SalesFixture.Reset();
        SeedStoreConfig();
    }

    public static User CreateTenantAdmin(int tenantId) =>
        User.Reconstitute(
            Guid.Parse($"00000000-0000-0000-0000-{tenantId:D12}"),
            tenantId,
            TenantId.Create(tenantId).Value,
            $"Tenant {tenantId}",
            Email.Create($"tenant{tenantId}@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            null,
            UserLevel.Administrador,
            ActiveStatus.Active,
            IntegrationTestUsers.Grants("vendas", "produtos", "categorias", "configuracoes")).Value;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IProductRepository>();
            services.RemoveAll<ICategoryRepository>();
            services.RemoveAll<IStockMovementRepository>();
            services.RemoveAll<IProductQueries>();
            services.RemoveAll<ICategoryQueries>();
            services.RemoveAll<ICatalogLegacyPort>();
            services.RemoveAll<IStockLegacyPort>();

            services.AddScoped<IProductRepository>(_ => CatalogFixture.ProductRepository);
            services.AddScoped<ICategoryRepository>(_ => CatalogFixture.CategoryRepository);
            services.AddScoped<IStockMovementRepository>(_ => CatalogFixture.StockMovementRepository);
            services.AddScoped<IProductQueries>(_ => CatalogFixture.ProductQueries);
            services.AddScoped<ICategoryQueries>(_ => CatalogFixture.CategoryQueries);
            services.AddScoped<ICatalogLegacyPort>(_ => CatalogFixture.LegacyPort);
            services.AddScoped<IStockLegacyPort>(_ => CatalogFixture.StockLegacyPort);

            services.RemoveAll<IStoreConfigRepository>();
            services.RemoveAll<IPaymentMethodRepository>();
            services.RemoveAll<ICashRegisterTerminalRepository>();
            services.AddScoped<IStoreConfigRepository>(_ => StoreSettingsFixture.StoreConfigRepository);
            services.AddScoped<IPaymentMethodRepository>(_ => StoreSettingsFixture.PaymentMethodRepository);
            services.AddScoped<ICashRegisterTerminalRepository>(_ => StoreSettingsFixture.CashRegisterTerminalRepository);

            services.RemoveAll<ICashSessionRepository>();
            services.RemoveAll<ISaleRepository>();
            services.RemoveAll<IListSalesQuery>();
            services.RemoveAll<ISalesLegacyPort>();
            services.RemoveAll<IManagerPinVerifier>();

            services.AddScoped<ICashSessionRepository>(_ => SalesFixture.CashSessionRepository);
            services.AddScoped<ISaleRepository>(_ => SalesFixture.SaleRepository);
            services.AddScoped<IListSalesQuery>(_ => SalesFixture.SalesQueries);
            services.AddScoped<ISalesLegacyPort>(_ => SalesFixture.LegacyPort);
            services.AddScoped<IManagerPinVerifier, ManagerPinVerifierStub>();
        });
    }

    private void SeedStoreConfig()
    {
        var tenantId = TenantId.Create(1).Value;
        var config = StoreConfig.Create(
            tenantId,
            StoreName.Create("Loja Teste").Value,
            null,
            DiscountType.Percentage,
            0m,
            CommissionRate.Create(2).Value,
            ReportFormat.Create("PDF").Value).Value;

        StoreSettingsFixture.StoreConfigRepository.Save(config).GetAwaiter().GetResult();
    }
}
