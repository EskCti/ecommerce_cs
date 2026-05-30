using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RetailOps.IntegrationTests.Support;

namespace RetailOps.IntegrationTests.Api;

public class FinanceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FinanceEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact(Skip = "Requires authenticated tenant session — extend with auth helper")]
    public async Task CreateReceivable_ThenSettle_ReturnsSettledStatus()
    {
        var payload = new
        {
            description = "Conta teste E2E",
            amount = 150m,
            dueDate = DateTime.UtcNow.AddDays(5),
        };

        var create = await _client.PostAsJsonAsync("/api/finance/receivables", payload);
        create.EnsureSuccessStatusCode();

        var created = await create.Content.ReadFromJsonAsync<ReceivableResponse>();
        Assert.NotNull(created);

        var settle = await _client.PostAsJsonAsync(
            $"/api/finance/receivables/{created!.Id}/settle",
            new { settlementDate = DateTime.UtcNow });

        settle.EnsureSuccessStatusCode();
        var settled = await settle.Content.ReadFromJsonAsync<ReceivableResponse>();
        Assert.Equal("Settled", settled!.Status);
    }

    private sealed record ReceivableResponse(Guid Id, string Status);
}
