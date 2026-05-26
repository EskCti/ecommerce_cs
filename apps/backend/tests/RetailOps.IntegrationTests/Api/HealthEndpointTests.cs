using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RetailOps.IntegrationTests.Support;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class HealthEndpointTests : IClassFixture<RetailOpsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(RetailOpsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("healthy", body.Status);
    }

    private sealed record HealthResponse(string Status, string Service);
}
