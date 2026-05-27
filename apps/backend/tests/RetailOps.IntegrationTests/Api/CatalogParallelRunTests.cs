using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RetailOps.IntegrationTests.Support;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

/// <summary>
/// Placeholder for parallel-run stock read comparison (EP-005 US-050).
/// Full implementation will compare in-memory catalog stock vs legacy ACL read model.
/// </summary>
public class CatalogParallelRunTests : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CatalogWebApplicationFactory _factory;

    public CatalogParallelRunTests(CatalogWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StockRead_Placeholder_ComparesNewAndLegacySources()
    {
        AuthWebApplicationFactory.ConfigureUser(CatalogWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");

        var categoryId = await CreateCategory(token);
        await CreateProduct(token, categoryId, "7891111111111", initialStock: 12);

        var newSystemStock = await AuthorizedGet("/api/catalog/products", token);
        Assert.Equal(HttpStatusCode.OK, newSystemStock.StatusCode);
        var page = await newSystemStock.Content.ReadFromJsonAsync<JsonElement>();
        var items = page.GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal(12, items[0].GetProperty("stock").GetInt32());

        var legacyStock = _factory.Fixture.State.Products.Values.Single().Stock.Value;
        Assert.Equal(12, legacyStock);

        // TODO(EP-005): replace with dedicated parallel-run compare endpoint/report.
    }

    private async Task<string> LoginToken(string login)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { login, password = "secret" });
        loginResponse.EnsureSuccessStatusCode();
        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private async Task<Guid> CreateCategory(string token)
    {
        var response = await AuthorizedPost("/api/catalog/categories", token, new { name = "Parallel", isActive = true });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task CreateProduct(string token, Guid categoryId, string barcode, int initialStock)
    {
        var response = await AuthorizedPost("/api/catalog/products", token, new
        {
            barcode,
            name = "Parallel SKU",
            salePrice = 10m,
            costPrice = 5m,
            initialStock,
            stockAlertLevel = 1,
            categoryId,
        });
        response.EnsureSuccessStatusCode();
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
}
