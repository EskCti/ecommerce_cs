using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using RetailOps.IntegrationTests.Support;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class MigrationStatusEndpointTests : IClassFixture<RetailOpsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MigrationStatusEndpointTests(RetailOpsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMigrationStatus_AsPlatform_ReturnsOk()
    {
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", "0");

        var response = await _client.GetAsync("/api/admin/migration-status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
