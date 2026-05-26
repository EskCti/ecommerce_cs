using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetailOps.Core.Crm.Application.Ports;
using RetailOps.Core.Crm.Application.Queries;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.IntegrationTests.Support;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class CrmEndpointTests : IClassFixture<CrmWebApplicationFactory>
{
    private const string ValidCpf = "52998224725";
    private const string ValidCnpj = "12345678000195";

    private readonly HttpClient _client;
    private readonly CrmWebApplicationFactory _factory;

    public CrmEndpointTests(CrmWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/crm/customers");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CustomerCrud_WorksForTenant()
    {
        AuthWebApplicationFactory.ConfigureUser(CrmWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");

        var create = await AuthorizedPost("/api/crm/customers", token, new
        {
            name = "João Silva",
            cpf = ValidCpf,
            email = "joao@test.com",
            phone = "11999999999",
            isActive = true,
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        var list = await AuthorizedGet("/api/crm/customers", token);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var get = await AuthorizedGet($"/api/crm/customers/{id}", token);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var update = await AuthorizedPut($"/api/crm/customers/{id}", token, new
        {
            name = "João Atualizado",
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var deactivate = await AuthorizedDelete($"/api/crm/customers/{id}", token);
        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);
    }

    [Fact]
    public async Task FindOrCreateByCpf_IsIdempotent()
    {
        AuthWebApplicationFactory.ConfigureUser(CrmWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");

        var first = await AuthorizedPost("/api/crm/customers/find-or-create-by-cpf", token, new
        {
            cpf = ValidCpf,
            name = "Cliente PDV",
        });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstBody = await first.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(firstBody.GetProperty("created").GetBoolean());
        var firstId = firstBody.GetProperty("customer").GetProperty("id").GetGuid();

        var second = await AuthorizedPost("/api/crm/customers/find-or-create-by-cpf", token, new
        {
            cpf = ValidCpf,
            name = "Cliente PDV",
        });
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondBody = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(secondBody.GetProperty("created").GetBoolean());
        var secondId = secondBody.GetProperty("customer").GetProperty("id").GetGuid();

        Assert.Equal(firstId, secondId);
    }

    [Fact]
    public async Task UpdateCustomer_FromOtherTenant_ReturnsBadRequest()
    {
        AuthWebApplicationFactory.ConfigureUser(CrmWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();

        var otherTenantCustomer = Customer.Create(
            TenantId.Create(2).Value,
            PersonName.Create("Outro").Value,
            RetailOps.Core.Crm.Domain.ValueObjects.Cpf.Create("39053344705").Value).Value;
        await _factory.Fixture.CustomerRepository.Save(otherTenantCustomer);

        var token = await LoginToken("tenant1@test.com");
        var response = await AuthorizedPut(
            $"/api/crm/customers/{otherTenantCustomer.Id}",
            token,
            new { name = "Hack" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SupplierCrud_WorksForTenant()
    {
        AuthWebApplicationFactory.ConfigureUser(CrmWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();
        var token = await LoginToken("tenant1@test.com");

        var create = await AuthorizedPost("/api/crm/suppliers", token, new
        {
            name = "Fornecedor LTDA",
            personType = 1,
            taxDocument = ValidCnpj,
            email = "fornecedor@test.com",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var list = await AuthorizedGet("/api/crm/suppliers", token);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
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

public class CrmWebApplicationFactory : AuthWebApplicationFactory
{
    internal InMemoryCrmFixture Fixture { get; } = new();

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
            IntegrationTestUsers.Grants("clientes", "fornecedores")).Value;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICustomerRepository>();
            services.RemoveAll<ISupplierRepository>();
            services.RemoveAll<ICustomerQueries>();
            services.RemoveAll<ISupplierQueries>();
            services.RemoveAll<ICrmLegacyPort>();

            services.AddScoped<ICustomerRepository>(_ => Fixture.CustomerRepository);
            services.AddScoped<ISupplierRepository>(_ => Fixture.SupplierRepository);
            services.AddScoped<ICustomerQueries>(_ => Fixture.CustomerQueries);
            services.AddScoped<ISupplierQueries>(_ => Fixture.SupplierQueries);
            services.AddScoped<ICrmLegacyPort>(_ => Fixture.LegacyPort);
        });
    }
}
