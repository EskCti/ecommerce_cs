using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetailOps.Core.Reporting.Application.DTOs;
using RetailOps.Core.Reporting.Application.Ports;
using RetailOps.Core.Reporting.Application.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.IntegrationTests.Api;
using RetailOps.IntegrationTests.Support;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class ReportingEndpointTests : IClassFixture<ReportingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReportingEndpointTests(ReportingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSalesReport_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/reporting/sales?from=2026-07-01&to=2026-07-31");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSalesReport_InvalidDateRange_ReturnsBadRequest()
    {
        AuthWebApplicationFactory.ConfigureUser(ReportingWebApplicationFactory.CreateTenantAdmin(1));
        var token = await LoginToken("tenant1@test.com");

        var response = await AuthorizedGet("/api/reporting/sales?from=2026-07-31&to=2026-07-01", token);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReportingPdfEndpoints_ReturnPdfContentType()
    {
        AuthWebApplicationFactory.ConfigureUser(ReportingWebApplicationFactory.CreateTenantAdmin(1));
        var token = await LoginToken("tenant1@test.com");
        var saleId = Guid.Parse("00000000-0000-0000-0016-000000000001");

        var endpoints = new[]
        {
            "/api/reporting/sales?from=2026-07-01&to=2026-07-31",
            "/api/reporting/low-stock",
            "/api/reporting/cash-sessions?from=2026-07-01&to=2026-07-31",
            "/api/reporting/profit?from=2026-07-01&to=2026-07-31",
            $"/api/reporting/receipts/{saleId}",
        };

        foreach (var path in endpoints)
        {
            var response = await AuthorizedGet(path, token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Assert.NotEmpty(bytes);
        }
    }

    private async Task<string> LoginToken(string email)
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { login = email, password = "secret" });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private Task<HttpResponseMessage> AuthorizedGet(string path, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }
}

public class ReportingWebApplicationFactory : AuthWebApplicationFactory
{
    internal InMemoryReportingFixture ReportingFixture { get; } = new();

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
            IntegrationTestUsers.Grants("rel_vendas", "rel_estoque", "rel_financeiro", "rel_caixa", "vendas")).Value;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IReportingLegacyPort>();
            services.RemoveAll<IReportingStoreBrandingPort>();
            services.RemoveAll<IStoreConfigRepository>();

            services.AddScoped<IReportingLegacyPort>(_ => ReportingFixture.LegacyPort);
            services.AddScoped<IReportingStoreBrandingPort>(_ => ReportingFixture.BrandingPort);
            services.AddScoped<IStoreConfigRepository>(_ => ReportingFixture.StoreConfigRepository);
        });
    }
}

internal sealed class InMemoryReportingFixture
{
    internal InMemoryReportingLegacyPort LegacyPort { get; } = new();
    internal InMemoryReportingBrandingPort BrandingPort { get; } = new();
    internal InMemoryStoreConfigRepository StoreConfigRepository { get; } = new();
}

internal sealed class InMemoryReportingBrandingPort : IReportingStoreBrandingPort
{
    public Task<Result<StoreBrandingDto>> GetBrandingAsync(TenantId tenantId, CancellationToken ct = default) =>
        Task.FromResult(Result<StoreBrandingDto>.Success(new StoreBrandingDto("Loja Teste", null, "PDF")));
}

internal sealed class InMemoryReportingLegacyPort : IReportingLegacyPort
{
    public Task<Result<SalesReportDocument>> GetSalesReportAsync(
        TenantId tenantId,
        ReportFilter filter,
        StoreBrandingDto branding,
        CancellationToken ct = default) =>
        Task.FromResult(Result<SalesReportDocument>.Success(new SalesReportDocument(
            branding.StoreName,
            branding.LogoPath,
            filter.DateRange.From,
            filter.DateRange.To,
            new List<SalesReportRow>
            {
                new(DateTime.UtcNow, "Cliente", "Vendedor", "Dinheiro", "Pago", 100, 0, 100),
            },
            100)));

    public Task<Result<LowStockReportDocument>> GetLowStockReportAsync(
        TenantId tenantId,
        StoreBrandingDto branding,
        CancellationToken ct = default) =>
        Task.FromResult(Result<LowStockReportDocument>.Success(new LowStockReportDocument(
            branding.StoreName,
            branding.LogoPath,
            new List<LowStockRow> { new("001", "Produto", 1, 5) })));

    public Task<Result<CashSessionsReportDocument>> GetCashSessionsReportAsync(
        TenantId tenantId,
        ReportFilter filter,
        StoreBrandingDto branding,
        CancellationToken ct = default) =>
        Task.FromResult(Result<CashSessionsReportDocument>.Success(new CashSessionsReportDocument(
            branding.StoreName,
            branding.LogoPath,
            filter.DateRange.From,
            filter.DateRange.To,
            new List<CashSessionReportRow>
            {
                new(DateTime.UtcNow, DateTime.UtcNow, "Caixa 1", "Operador", 100, 500, 500, 0),
            })));

    public Task<Result<ProfitReportDocument>> GetProfitReportAsync(
        TenantId tenantId,
        ReportFilter filter,
        StoreBrandingDto branding,
        CancellationToken ct = default) =>
        Task.FromResult(Result<ProfitReportDocument>.Success(new ProfitReportDocument(
            branding.StoreName,
            branding.LogoPath,
            filter.DateRange.From,
            filter.DateRange.To,
            new ProfitStatement(1000, 400, 200, 400, false))));

    public Task<Result<ReceiptDocument>> GetReceiptAsync(
        TenantId tenantId,
        Guid saleId,
        StoreBrandingDto branding,
        CancellationToken ct = default)
    {
        if (saleId == Guid.Empty)
            return Task.FromResult(Result<ReceiptDocument>.Failure("Sale not found."));

        return Task.FromResult(Result<ReceiptDocument>.Success(new ReceiptDocument(
            saleId,
            branding.StoreName,
            branding.LogoPath,
            "Rua Teste",
            "contato@test.com",
            DateTime.UtcNow,
            "Dinheiro",
            "À vista",
            "Cliente",
            new List<ReceiptLine> { new("789", "Produto", 1, 10, 10) },
            10,
            0,
            10,
            10,
            0)));
    }
}

internal sealed class InMemoryStoreConfigRepository : IStoreConfigRepository
{
    public Task<Result<RetailOps.Core.StoreSettings.Domain.Entities.StoreConfig>> GetById(Guid id) =>
        Task.FromResult(Result<RetailOps.Core.StoreSettings.Domain.Entities.StoreConfig>.Failure("Not used"));

    public Task<Result<RetailOps.Core.StoreSettings.Domain.Entities.StoreConfig>> GetByTenantId(TenantId tenantId) =>
        Task.FromResult(Result<RetailOps.Core.StoreSettings.Domain.Entities.StoreConfig>.Failure("Not configured in tests"));

    public Task<Result<bool>> ExistsForTenant(TenantId tenantId) =>
        Task.FromResult(Result<bool>.Success(false));

    public Task<Result> Save(RetailOps.Core.StoreSettings.Domain.Entities.StoreConfig entity) =>
        Task.FromResult(Result.Success());

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Success());
}
