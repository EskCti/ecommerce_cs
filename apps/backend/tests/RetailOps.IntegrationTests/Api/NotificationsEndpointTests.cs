using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetailOps.Core.Notifications.Application.Ports;
using RetailOps.Core.Notifications.Domain.Repositories;
using RetailOps.Core.Notifications.Domain.ValueObjects;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.IntegrationTests.Api;
using RetailOps.IntegrationTests.Support;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.IntegrationTests.Api;

public class NotificationsEndpointTests : IClassFixture<NotificationsWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly NotificationsWebApplicationFactory _factory;

    public NotificationsEndpointTests(NotificationsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TriggerDigest_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync("/api/notifications/digest/trigger?tenantId=1", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TriggerDigest_AsAdmin_SendsAndPersistsLog()
    {
        AuthWebApplicationFactory.ConfigureUser(NotificationsWebApplicationFactory.CreateTenantAdmin(1));
        _factory.Fixture.Reset();

        var token = await LoginToken("tenant1@test.com");
        var first = await AuthorizedPost("/api/notifications/digest/trigger?tenantId=1", token);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstBody = await first.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("sent", firstBody.GetProperty("status").GetString());
        Assert.Equal(1, _factory.Fixture.Gateway.CallCount);
        Assert.Equal(1, _factory.Fixture.SavedLogs);

        var second = await AuthorizedPost("/api/notifications/digest/trigger?tenantId=1", token);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondBody = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("skipped:already_sent_today", secondBody.GetProperty("status").GetString());
        Assert.Equal(1, _factory.Fixture.Gateway.CallCount);
    }

    private async Task<string> LoginToken(string email)
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { login = email, password = "secret" });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private Task<HttpResponseMessage> AuthorizedPost(string path, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }
}

public class NotificationsWebApplicationFactory : AuthWebApplicationFactory
{
    internal InMemoryNotificationsFixture Fixture { get; } = new();

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

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IWhatsAppGatewayPort>();
            services.RemoveAll<IWhatsAppCredentialsResolver>();
            services.RemoveAll<INotificationStoreNamePort>();
            services.RemoveAll<IReceivablesDueTodayQueryPort>();
            services.RemoveAll<ILowStockSummaryQueryPort>();
            services.RemoveAll<ITenantBillingAlertQueryPort>();
            services.RemoveAll<IDailyDigestLogRepository>();

            services.AddScoped<IWhatsAppGatewayPort>(_ => Fixture.Gateway);
            services.AddScoped<IWhatsAppCredentialsResolver>(_ => Fixture.CredentialsResolver);
            services.AddScoped<INotificationStoreNamePort>(_ => Fixture.StoreNamePort);
            services.AddScoped<IReceivablesDueTodayQueryPort>(_ => Fixture.QueryPort);
            services.AddScoped<ILowStockSummaryQueryPort>(_ => Fixture.QueryPort);
            services.AddScoped<ITenantBillingAlertQueryPort>(_ => Fixture.QueryPort);
            services.AddScoped<IDailyDigestLogRepository>(_ => Fixture.LogRepository);
        });
    }
}

internal sealed class InMemoryNotificationsFixture
{
    internal RecordingWhatsAppGateway Gateway { get; } = new();
    internal InMemoryDigestLogRepository LogRepository { get; } = new();
    internal StubCredentialsResolver CredentialsResolver { get; } = new();
    internal StubStoreNamePort StoreNamePort { get; } = new();
    internal StubDigestQueryPort QueryPort { get; } = new();

    public int SavedLogs => LogRepository.SavedCount;

    public void Reset()
    {
        Gateway.CallCount = 0;
        LogRepository.Reset();
    }
}

internal sealed class RecordingWhatsAppGateway : IWhatsAppGatewayPort
{
    public int CallCount { get; set; }

    public Task<Result> SendTextAsync(
        PhoneNumber to,
        MessageTemplate body,
        WhatsAppCredentials credentials,
        CancellationToken ct = default)
    {
        CallCount++;
        return Task.FromResult(Result.Success());
    }
}

internal sealed class InMemoryDigestLogRepository : IDailyDigestLogRepository
{
    private readonly HashSet<(int TenantId, DateOnly Date)> _sent = [];

    public int SavedCount { get; private set; }

    public Task<Result<bool>> ExistsForDateAsync(
        TenantId tenantId,
        DateOnly digestDate,
        CancellationToken ct = default) =>
        Task.FromResult(Result<bool>.Success(_sent.Contains((tenantId.Value, digestDate))));

    public Task<Result> SaveAsync(
        Core.Notifications.Domain.Entities.DailyDigestLog log,
        CancellationToken ct = default)
    {
        _sent.Add((log.TenantId.Value, log.DigestDate));
        SavedCount++;
        return Task.FromResult(Result.Success());
    }

    public void Reset()
    {
        _sent.Clear();
        SavedCount = 0;
    }
}

internal sealed class StubCredentialsResolver : IWhatsAppCredentialsResolver
{
    public Task<Result<WhatsAppCredentials>> ResolveAsync(TenantId tenantId, CancellationToken ct = default)
    {
        var phone = PhoneNumber.Create("11999998888").Value;
        return Task.FromResult(WhatsAppCredentials.Create("1234567890", null, phone));
    }
}

internal sealed class StubStoreNamePort : INotificationStoreNamePort
{
    public Task<Result<string>> GetStoreNameAsync(TenantId tenantId, CancellationToken ct = default) =>
        Task.FromResult(Result<string>.Success("Loja Teste"));
}

internal sealed class StubDigestQueryPort
    : IReceivablesDueTodayQueryPort, ILowStockSummaryQueryPort, ITenantBillingAlertQueryPort
{
    public Task<Result<ReceivablesDueTodaySummary>> GetDueTodaySummaryAsync(
        TenantId tenantId,
        CancellationToken ct = default) =>
        Task.FromResult(Result<ReceivablesDueTodaySummary>.Success(new ReceivablesDueTodaySummary(1, 100m)));

    public Task<Result<LowStockSummary>> GetLowStockSummaryAsync(
        TenantId tenantId,
        CancellationToken ct = default) =>
        Task.FromResult(Result<LowStockSummary>.Success(new LowStockSummary(1, ["Produto X"])));

    public Task<Result<string?>> GetBillingAlertAsync(
        TenantId tenantId,
        CancellationToken ct = default) =>
        Task.FromResult(Result<string?>.Success("Mensalidade vence hoje."));
}
