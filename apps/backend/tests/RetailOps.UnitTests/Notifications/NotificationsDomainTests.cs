using RetailOps.Core.Notifications.Application.Ports;
using RetailOps.Core.Notifications.Application.UseCases;
using RetailOps.Core.Notifications.Domain.Repositories;
using RetailOps.Core.Notifications.Domain.Services;
using RetailOps.Core.Notifications.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Notifications;

public class DailyDigestComposerTests
{
    [Fact]
    public async Task ComposeAsync_AggregatesAllSections()
    {
        var composer = CreateComposer(2, 150m, 3, ["Prod A"], "Mensalidade vence hoje.");

        var result = await composer.ComposeAsync(TenantId.Create(1).Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.ReceivablesDueTodayCount);
        Assert.Equal(3, result.Value.LowStockProductCount);
        Assert.NotNull(result.Value.BillingAlertText);
    }

    [Fact]
    public void FormatMessage_IncludesReceivablesAndLowStock()
    {
        var composer = CreateComposer(0, 0, 0, [], null);
        var content = new DigestContent
        {
            ReceivablesDueTodayCount = 1,
            ReceivablesDueTodayTotal = 50m,
            LowStockProductCount = 2,
            TopLowStockProductNames = ["Item 1"],
        };

        var message = composer.FormatMessage(content, "Loja Teste");

        Assert.True(message.IsSuccess);
        Assert.Contains("Contas vencendo hoje", message.Value.Text);
        Assert.Contains("Estoque baixo", message.Value.Text);
    }

    [Fact]
    public void WhatsAppCredentials_TenantTokenOverridesGlobal()
    {
        var phone = PhoneNumber.Create("11999998888").Value;
        var result = WhatsAppCredentials.Create("tenant-token-123456", "global-token-123456", phone);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.UsesTenantToken);
        Assert.Equal("tenant-token-123456", result.Value.ApiToken);
    }

    [Fact]
    public void WhatsAppCredentials_FallsBackToGlobalToken()
    {
        var phone = PhoneNumber.Create("11999998888").Value;
        var result = WhatsAppCredentials.Create(null, "global-token-123456", phone);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.UsesTenantToken);
    }

    [Fact]
    public void PhoneNumber_RejectsInvalidInput()
    {
        Assert.True(PhoneNumber.Create("123").IsFailure);
        Assert.True(PhoneNumber.Create("11999998888").IsSuccess);
    }

    [Fact]
    public async Task Handler_SkipsWhenAlreadySentToday()
    {
        var gateway = new RecordingWhatsAppGateway();
        var handler = new SendDailyTenantDigestHandler(
            new StubDigestLogRepository(exists: true),
            new StubCredentialsResolver(),
            new StubStoreNamePort(),
            CreateComposer(1, 10m, 0, [], null),
            gateway);

        var result = await handler.ExecuteAsync(TenantId.Create(1).Value);

        Assert.True(result.IsSuccess);
        Assert.Equal("skipped:already_sent_today", result.Value);
        Assert.Equal(0, gateway.CallCount);
        Assert.Null(handler.LastEvent);
    }

    private static DailyDigestComposer CreateComposer(
        int receivableCount,
        decimal receivableTotal,
        int lowStockCount,
        IReadOnlyList<string> lowStockNames,
        string? billingAlert) =>
        new(
            new StubReceivablesPort(receivableCount, receivableTotal),
            new StubLowStockPort(lowStockCount, lowStockNames),
            new StubBillingPort(billingAlert));

    private sealed class StubReceivablesPort(int count, decimal total) : IReceivablesDueTodayQueryPort
    {
        public Task<Result<ReceivablesDueTodaySummary>> GetDueTodaySummaryAsync(
            TenantId tenantId,
            CancellationToken ct = default) =>
            Task.FromResult(Result<ReceivablesDueTodaySummary>.Success(new ReceivablesDueTodaySummary(count, total)));
    }

    private sealed class StubLowStockPort(int count, IReadOnlyList<string> names) : ILowStockSummaryQueryPort
    {
        public Task<Result<LowStockSummary>> GetLowStockSummaryAsync(
            TenantId tenantId,
            CancellationToken ct = default) =>
            Task.FromResult(Result<LowStockSummary>.Success(new LowStockSummary(count, names)));
    }

    private sealed class StubBillingPort(string? alert) : ITenantBillingAlertQueryPort
    {
        public Task<Result<string?>> GetBillingAlertAsync(
            TenantId tenantId,
            CancellationToken ct = default) =>
            Task.FromResult(Result<string?>.Success(alert));
    }

    private sealed class StubDigestLogRepository(bool exists) : IDailyDigestLogRepository
    {
        public Task<Result<bool>> ExistsForDateAsync(
            TenantId tenantId,
            DateOnly digestDate,
            CancellationToken ct = default) =>
            Task.FromResult(Result<bool>.Success(exists));

        public Task<Result> SaveAsync(
            Core.Notifications.Domain.Entities.DailyDigestLog log,
            CancellationToken ct = default) =>
            Task.FromResult(Result.Success());
    }

    private sealed class StubCredentialsResolver : IWhatsAppCredentialsResolver
    {
        public Task<Result<WhatsAppCredentials>> ResolveAsync(TenantId tenantId, CancellationToken ct = default)
        {
            var phone = PhoneNumber.Create("11999998888").Value;
            return Task.FromResult(WhatsAppCredentials.Create("1234567890", null, phone));
        }
    }

    private sealed class StubStoreNamePort : INotificationStoreNamePort
    {
        public Task<Result<string>> GetStoreNameAsync(TenantId tenantId, CancellationToken ct = default) =>
            Task.FromResult(Result<string>.Success("Loja"));
    }

    private sealed class RecordingWhatsAppGateway : IWhatsAppGatewayPort
    {
        public int CallCount { get; private set; }

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
}
