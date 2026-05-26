using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Application.Queries;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.IntegrationTests.Api;
using RetailOps.IntegrationTests.Support;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class StoreSettingsEndpointTests : IClassFixture<StoreSettingsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly StoreSettingsWebApplicationFactory _factory;

    public StoreSettingsEndpointTests(StoreSettingsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStoreConfig_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/settings/store-config");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStoreConfig_WithTenantAdmin_CreatesDefaultOnFirstRead()
    {
        AuthWebApplicationFactory.ConfigureUser(StoreSettingsWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();

        var token = await LoginToken("tenant1@test.com");
        var response = await AuthorizedGet("/api/settings/store-config", token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Minha Loja", body.GetProperty("name").GetString());
        Assert.Equal(1, body.GetProperty("tenantId").GetInt32());
    }

    [Fact]
    public async Task PutStoreConfig_UpdatesConfiguration()
    {
        AuthWebApplicationFactory.ConfigureUser(StoreSettingsWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();

        var token = await LoginToken("tenant1@test.com");
        await AuthorizedGet("/api/settings/store-config", token);

        var put = await AuthorizedPut("/api/settings/store-config", token, new
        {
            name = "Loja Atualizada",
            discountType = "Percentage",
            discountValue = 5,
            commissionRate = 2,
            reportFormat = "PDF",
            contacts = "contato@loja.com",
            address = "Rua 1",
        });

        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        var body = await put.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Loja Atualizada", body.GetProperty("name").GetString());
        Assert.Equal("contato@loja.com", body.GetProperty("contacts").GetString());
    }

    [Fact]
    public async Task PaymentMethodCrud_WorksForTenant()
    {
        AuthWebApplicationFactory.ConfigureUser(StoreSettingsWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");

        var create = await AuthorizedPost("/api/settings/payment-methods", token, new
        {
            name = "Pix",
            surchargePercent = 2.5m,
            isActive = true,
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        var list = await AuthorizedGet("/api/settings/payment-methods", token);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var items = await list.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, items.GetArrayLength());

        var update = await AuthorizedPut($"/api/settings/payment-methods/{id}", token, new
        {
            name = "Pix Atualizado",
            surchargePercent = 3m,
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var delete = await AuthorizedDelete($"/api/settings/payment-methods/{id}", token);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task UpdatePaymentMethod_FromOtherTenant_ReturnsBadRequest()
    {
        AuthWebApplicationFactory.ConfigureUser(StoreSettingsWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();

        var otherTenantMethod = PaymentMethod.Reconstitute(
            Guid.Parse("00000000-0000-0000-0004-000000000099"),
            TenantId.Create(2).Value,
            PaymentMethodName.Create("Fiado").Value,
            SurchargePercent.Create(0).Value,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow).Value;
        await _factory.Fixture.PaymentMethodRepository.Save(otherTenantMethod);

        var token = await LoginToken("tenant1@test.com");
        var response = await AuthorizedPut(
            $"/api/settings/payment-methods/{otherTenantMethod.Id}",
            token,
            new { name = "Hack" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStoreConfig_WithoutTenantContext_ReturnsForbidden()
    {
        AuthWebApplicationFactory.ConfigureUser(StoreSettingsWebApplicationFactory.CreatePlatformSasUser());
        _factory.Fixture.Reset();

        var token = await LoginToken("sas@test.com");
        var response = await AuthorizedGet("/api/settings/store-config", token);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
}

public class StoreSettingsWebApplicationFactory : AuthWebApplicationFactory
{
    internal InMemoryStoreSettingsFixture Fixture { get; } = new();

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
            IntegrationTestUsers.Grants("configuracoes")).Value;

    public static User CreatePlatformSasUser() =>
        User.Reconstitute(
            Guid.Parse("00000000-0000-0000-0000-000000000888"),
            888,
            TenantId.Platform,
            "SAS",
            Email.Create("sas@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            null,
            UserLevel.Sas,
            ActiveStatus.Active,
            []).Value;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IStoreConfigRepository>();
            services.RemoveAll<IPaymentMethodRepository>();
            services.RemoveAll<ICashRegisterTerminalRepository>();
            services.RemoveAll<IPaymentMethodQueries>();
            services.RemoveAll<ICashRegisterTerminalQueries>();

            services.AddScoped<IStoreConfigRepository>(_ => Fixture.StoreConfigRepository);
            services.AddScoped<IPaymentMethodRepository>(_ => Fixture.PaymentMethodRepository);
            services.AddScoped<ICashRegisterTerminalRepository>(_ => Fixture.CashRegisterTerminalRepository);
            services.AddScoped<IPaymentMethodQueries>(_ => Fixture.PaymentMethodQueries);
            services.AddScoped<ICashRegisterTerminalQueries>(_ => Fixture.CashRegisterTerminalQueries);
        });
    }
}
