using System.Net;
using RetailOps.IntegrationTests.Support;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class CutoverEndpointTests : IClassFixture<RetailOpsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CutoverEndpointTests(RetailOpsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCutoverChecklist_AsPlatform_ReturnsOk()
    {
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", "0");

        var response = await _client.GetAsync("/api/admin/cutover-checklist");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMigrationStatus_IncludesCutoverFields()
    {
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", "0");

        var response = await _client.GetAsync("/api/admin/migration-status");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("legacyPhpEnabled", body);
        Assert.Contains("tenantsOnPhpPath", body);
    }
}
