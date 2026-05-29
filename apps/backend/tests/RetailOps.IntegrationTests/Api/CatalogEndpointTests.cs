using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Application.Queries;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.IntegrationTests.Support;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class CatalogEndpointTests : IClassFixture<CatalogWebApplicationFactory>
{
    private const string ProductBarcode = "7891234567890";

    private readonly HttpClient _client;
    private readonly CatalogWebApplicationFactory _factory;

    public CatalogEndpointTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/catalog/products");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProductCrud_AndBarcodeLookup_WorkForTenant()
    {
        AuthWebApplicationFactory.ConfigureUser(CatalogWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");
        var categoryId = await CreateCategory(token, "Eletrônicos");

        var create = await AuthorizedPost("/api/catalog/products", token, new
        {
            barcode = ProductBarcode,
            name = "Mouse USB",
            salePrice = 29.9m,
            costPrice = 15m,
            initialStock = 20,
            stockAlertLevel = 5,
            categoryId,
            isActive = true,
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        var list = await AuthorizedGet("/api/catalog/products", token);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var get = await AuthorizedGet($"/api/catalog/products/{id}", token);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var byBarcode = await AuthorizedGet($"/api/catalog/products/by-barcode/{ProductBarcode}", token);
        Assert.Equal(HttpStatusCode.OK, byBarcode.StatusCode);
        var byBarcodeBody = await byBarcode.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(id, byBarcodeBody.GetProperty("id").GetGuid());

        var update = await AuthorizedPut($"/api/catalog/products/{id}", token, new
        {
            name = "Mouse USB Pro",
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var deactivate = await AuthorizedDelete($"/api/catalog/products/{id}", token);
        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateBarcode_ReturnsConflict()
    {
        AuthWebApplicationFactory.ConfigureUser(CatalogWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");
        var categoryId = await CreateCategory(token, "Papelaria");

        var first = await AuthorizedPost("/api/catalog/products", token, new
        {
            barcode = ProductBarcode,
            name = "Caderno",
            salePrice = 12m,
            costPrice = 6m,
            initialStock = 10,
            stockAlertLevel = 2,
            categoryId,
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await AuthorizedPost("/api/catalog/products", token, new
        {
            barcode = ProductBarcode,
            name = "Outro Caderno",
            salePrice = 14m,
            costPrice = 7m,
            initialStock = 5,
            stockAlertLevel = 2,
            categoryId,
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var body = await duplicate.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("DUPLICATE_BARCODE", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task GetByBarcode_WhenNotFound_ReturnsNotFound()
    {
        AuthWebApplicationFactory.ConfigureUser(CatalogWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");

        var response = await AuthorizedGet("/api/catalog/products/by-barcode/0000000000000", token);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CategoryCrud_WorksForTenant()
    {
        AuthWebApplicationFactory.ConfigureUser(CatalogWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");

        var create = await AuthorizedPost("/api/catalog/categories", token, new
        {
            name = "Bebidas",
            isActive = true,
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        var list = await AuthorizedGet("/api/catalog/categories", token);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var get = await AuthorizedGet($"/api/catalog/categories/{id}", token);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var update = await AuthorizedPut($"/api/catalog/categories/{id}", token, new
        {
            name = "Bebidas Geladas",
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var deactivate = await AuthorizedDelete($"/api/catalog/categories/{id}", token);
        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);
    }

    private async Task<Guid> CreateCategory(string token, string name)
    {
        var response = await AuthorizedPost("/api/catalog/categories", token, new
        {
            name,
            isActive = true,
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task<string> LoginToken(string login)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { login, password = "secret" });
        loginResponse.EnsureSuccessStatusCode();
        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private Task<HttpResponseMessage> AuthorizedGet(string url, string token)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(req);
    }

    private Task<HttpResponseMessage> AuthorizedPost(string url, string token, object body)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(req);
    }

    private Task<HttpResponseMessage> AuthorizedPut(string url, string token, object body)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(body),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(req);
    }

    private Task<HttpResponseMessage> AuthorizedDelete(string url, string token)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(req);
    }

    [Fact]
    public async Task GradeMatrix_TwoDimensions_ExposesVariantsWithStock()
    {
        AuthWebApplicationFactory.ConfigureUser(CatalogWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");
        var categoryId = await CreateCategory(token, "Vestuário");

        var create = await AuthorizedPost("/api/catalog/products", token, new
        {
            barcode = "7899999999991",
            name = "Camiseta 2D",
            salePrice = 49.9m,
            costPrice = 20m,
            initialStock = 0,
            stockAlertLevel = 2,
            categoryId,
            isActive = true,
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var productId = (await create.Content.ReadFromJsonAsync<JsonElement>())!.GetProperty("id").GetGuid();

        await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "AddDimension",
            dimensionName = "Cor",
        });
        await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "AddDimension",
            dimensionName = "Tamanho",
        });
        await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "AddOption",
            dimensionId = (await GetFirstDimensionId(productId, token, "Cor")),
            optionLabel = "Azul",
            stock = 0,
        });
        await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "AddOption",
            dimensionId = (await GetFirstDimensionId(productId, token, "Cor")),
            optionLabel = "Vermelho",
            stock = 0,
        });
        var tamanhoDim = await GetFirstDimensionId(productId, token, "Tamanho");
        await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "AddOption",
            dimensionId = tamanhoDim,
            optionLabel = "P",
            stock = 0,
        });
        await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "AddOption",
            dimensionId = tamanhoDim,
            optionLabel = "M",
            stock = 0,
        });

        var listAfterOptions = await AuthorizedGet($"/api/catalog/products/{productId}/grades", token);
        var configAfterOptions = await listAfterOptions.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, configAfterOptions!.GetProperty("variants").GetArrayLength());

        var azulId = await GetOptionId(productId, token, "Cor", "Azul");
        var pId = await GetOptionId(productId, token, "Tamanho", "P");

        var addVariant = await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "AddVariant",
            optionIds = new[] { azulId, pId },
            stock = 5,
        });
        Assert.Equal(HttpStatusCode.OK, addVariant.StatusCode);

        var list = await AuthorizedGet($"/api/catalog/products/{productId}/grades", token);
        var config = await list.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, config!.GetProperty("variants").GetArrayLength());
        Assert.Equal(5, config.GetProperty("variants")[0].GetProperty("stock").GetInt32());

        var variantId = config.GetProperty("variants")[0].GetProperty("id").GetGuid();
        var remove = await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "RemoveVariant",
            variantId,
        });
        Assert.Equal(HttpStatusCode.OK, remove.StatusCode);

        var addAgain = await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "AddVariant",
            optionIds = new[] { azulId, pId },
            stock = 2,
        });
        Assert.Equal(HttpStatusCode.OK, addAgain.StatusCode);

        var blockRemove = await AuthorizedPost($"/api/catalog/products/{productId}/grades", token, new
        {
            action = "RemoveGrade",
            optionId = azulId,
        });
        Assert.Equal(HttpStatusCode.BadRequest, blockRemove.StatusCode);
    }

    private async Task<Guid> GetOptionId(Guid productId, string token, string dimensionName, string optionLabel)
    {
        var list = await AuthorizedGet($"/api/catalog/products/{productId}/grades", token);
        var config = await list.Content.ReadFromJsonAsync<JsonElement>();
        foreach (var dim in config!.GetProperty("dimensions").EnumerateArray())
        {
            if (dim.GetProperty("name").GetString() != dimensionName)
                continue;

            foreach (var opt in dim.GetProperty("options").EnumerateArray())
            {
                if (opt.GetProperty("label").GetString() == optionLabel)
                    return opt.GetProperty("id").GetGuid();
            }
        }

        throw new InvalidOperationException($"Option {optionLabel} not found in {dimensionName}.");
    }

    private async Task<Guid> GetFirstDimensionId(Guid productId, string token, string name)
    {
        var list = await AuthorizedGet($"/api/catalog/products/{productId}/grades", token);
        var config = await list.Content.ReadFromJsonAsync<JsonElement>();
        foreach (var dim in config.GetProperty("dimensions").EnumerateArray())
        {
            if (dim.GetProperty("name").GetString() == name)
                return dim.GetProperty("id").GetGuid();
        }

        throw new InvalidOperationException($"Dimension {name} not found.");
    }
}

public class CatalogWebApplicationFactory : AuthWebApplicationFactory
{
    internal InMemoryCatalogFixture Fixture { get; } = new();

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
            IntegrationTestUsers.Grants("produtos", "categorias")).Value;

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

            services.AddScoped<IProductRepository>(_ => Fixture.ProductRepository);
            services.AddScoped<ICategoryRepository>(_ => Fixture.CategoryRepository);
            services.AddScoped<IStockMovementRepository>(_ => Fixture.StockMovementRepository);
            services.AddScoped<IProductQueries>(_ => Fixture.ProductQueries);
            services.AddScoped<ICategoryQueries>(_ => Fixture.CategoryQueries);
            services.AddScoped<ICatalogLegacyPort>(_ => Fixture.LegacyPort);
            services.AddScoped<IStockLegacyPort>(_ => Fixture.StockLegacyPort);
        });
    }
}
