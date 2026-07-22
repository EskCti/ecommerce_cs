using RetailOps.Core.Notifications.Application.Ports;
using RetailOps.Core.Notifications.Domain.Entities;
using RetailOps.Core.Notifications.Domain.Events;
using RetailOps.Core.Notifications.Domain.Repositories;
using RetailOps.Core.Notifications.Domain.Services;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Notifications.Application.UseCases;

public sealed class SendDailyTenantDigestHandler(
    IDailyDigestLogRepository digestLogRepository,
    IWhatsAppCredentialsResolver credentialsResolver,
    INotificationStoreNamePort storeNamePort,
    DailyDigestComposer composer,
    IWhatsAppGatewayPort whatsAppGateway)
{
    public DigestSent? LastEvent { get; private set; }

    public async Task<Result<string>> ExecuteAsync(TenantId tenantId, CancellationToken ct = default)
    {
        LastEvent = null;
        var digestDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var existsResult = await digestLogRepository.ExistsForDateAsync(tenantId, digestDate, ct);
        if (existsResult.IsFailure)
            return Result<string>.Failure(existsResult.Error);

        if (existsResult.Value)
            return Result<string>.Success("skipped:already_sent_today");

        var credentialsResult = await credentialsResolver.ResolveAsync(tenantId, ct);
        if (credentialsResult.IsFailure)
            return Result<string>.Success($"skipped:{credentialsResult.Error}");

        var storeNameResult = await storeNamePort.GetStoreNameAsync(tenantId, ct);
        if (storeNameResult.IsFailure)
            return Result<string>.Failure(storeNameResult.Error);

        var contentResult = await composer.ComposeAsync(tenantId, ct);
        if (contentResult.IsFailure)
            return Result<string>.Failure(contentResult.Error);

        var messageResult = composer.FormatMessage(contentResult.Value, storeNameResult.Value);
        if (messageResult.IsFailure)
            return Result<string>.Failure(messageResult.Error);

        var sendResult = await whatsAppGateway.SendTextAsync(
            credentialsResult.Value.DestinationPhone,
            messageResult.Value,
            credentialsResult.Value,
            ct);

        if (sendResult.IsFailure)
            return Result<string>.Failure(sendResult.Error);

        var logResult = DailyDigestLog.Record(tenantId, digestDate, DateTime.UtcNow);
        if (logResult.IsFailure)
            return Result<string>.Failure(logResult.Error);

        var saveResult = await digestLogRepository.SaveAsync(logResult.Value, ct);
        if (saveResult.IsFailure)
            return Result<string>.Failure(saveResult.Error);

        LastEvent = new DigestSent(tenantId, digestDate, DateTime.UtcNow);
        return Result<string>.Success("sent");
    }
}
