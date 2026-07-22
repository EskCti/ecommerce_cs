using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Crm.Application.Ports;
using RetailOps.Core.Crm.Application.Queries;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Returns.Application.Ports;
using RetailOps.Core.Returns.Domain.Repositories;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.IntegrationTests.Api;
using RetailOps.IntegrationTests.Support;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class ReturnsEndpointTests : IClassFixture<ReturnsWebApplicationFactory>
{
    private const string ValidCpf = "52998224725";

    private readonly HttpClient _client;
    private readonly ReturnsWebApplicationFactory _factory;

    public ReturnsEndpointTests(ReturnsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetExchanges_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/returns/exchanges");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterExchange_AdjustsStock_AndDeleteReverses()
    {
        AuthWebApplicationFactory.ConfigureUser(ReturnsWebApplicationFactory.CreateTenantAdmin(1));
        _factory.CatalogFixture.Reset();
        _factory.CrmFixture.Reset();
        _factory.ReturnsFixture.Reset();

        var token = await LoginToken("tenant1@test.com");
        var categoryId = await CreateCategory(token);
        var productInId = await CreateProduct(token, categoryId, "1111111111111", "Prod Entrada", 5);
        var productOutId = await CreateProduct(token, categoryId, "2222222222222", "Prod Saída", 2);

        var register = await AuthorizedPost("/api/returns/exchanges", token, new
        {
            cpf = ValidCpf,
            customerName = "Cliente Troca",
            productInId,
            productOutId,
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        var created = await register.Content.ReadFromJsonAsync<JsonElement>();
        var exchangeId = created.GetProperty("id").GetGuid();

        var productIn = await AuthorizedGet($"/api/catalog/products/{productInId}", token);
        var productOut = await AuthorizedGet($"/api/catalog/products/{productOutId}", token);
        var inBody = await productIn.Content.ReadFromJsonAsync<JsonElement>();
        var outBody = await productOut.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(6, inBody.GetProperty("stock").GetInt32());
        Assert.Equal(1, outBody.GetProperty("stock").GetInt32());

        var list = await AuthorizedGet("/api/returns/exchanges", token);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var delete = await AuthorizedDelete($"/api/returns/exchanges/{exchangeId}", token);
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);

        productIn = await AuthorizedGet($"/api/catalog/products/{productInId}", token);
        productOut = await AuthorizedGet($"/api/catalog/products/{productOutId}", token);
        inBody = await productIn.Content.ReadFromJsonAsync<JsonElement>();
        outBody = await productOut.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5, inBody.GetProperty("stock").GetInt32());
        Assert.Equal(2, outBody.GetProperty("stock").GetInt32());
    }

    private async Task<string> LoginToken(string email)
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { login = email, password = "secret" });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private async Task<Guid> CreateCategory(string token)
    {
        var response = await AuthorizedPost("/api/catalog/categories", token, new { name = "Cat Troca" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CreateProduct(string token, Guid categoryId, string barcode, string name, int stock)
    {
        var response = await AuthorizedPost("/api/catalog/products", token, new
        {
            barcode,
            name,
            salePrice = 10m,
            costPrice = 5m,
            initialStock = stock,
            stockAlertLevel = 1,
            categoryId,
            isActive = true,
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private Task<HttpResponseMessage> AuthorizedGet(string path, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }

    private Task<HttpResponseMessage> AuthorizedPost(string path, string token, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }

    private Task<HttpResponseMessage> AuthorizedDelete(string path, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }
}

public class ReturnsWebApplicationFactory : AuthWebApplicationFactory
{
    internal InMemoryCatalogFixture CatalogFixture { get; } = new();
    internal InMemoryCrmFixture CrmFixture { get; } = new();
    internal InMemoryReturnsFixture ReturnsFixture { get; } = new();

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
            IntegrationTestUsers.Grants("devolucoes", "produtos", "categorias", "clientes")).Value;

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

            services.RemoveAll<ICustomerRepository>();
            services.RemoveAll<ICustomerQueries>();
            services.RemoveAll<ICrmLegacyPort>();

            services.AddScoped<ICustomerRepository>(_ => CrmFixture.CustomerRepository);
            services.AddScoped<ICustomerQueries>(_ => CrmFixture.CustomerQueries);
            services.AddScoped<ICrmLegacyPort>(_ => CrmFixture.LegacyPort);

            services.RemoveAll<IExchangeRepository>();
            services.RemoveAll<IReturnsLegacyPort>();

            services.AddScoped<IExchangeRepository>(_ => ReturnsFixture.ExchangeRepository);
            services.AddScoped<IReturnsLegacyPort>(_ => ReturnsFixture.ExchangeRepository);
        });
    }
}
