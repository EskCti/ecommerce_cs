using Microsoft.Extensions.Options;
using RetailOps.Core.Notifications.Application.Ports;
using RetailOps.Core.Notifications.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Notifications;

public sealed class WhatsAppSettings
{
    public const string SectionName = "WhatsApp";

    public string DefaultToken { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = "https://api.enviame.com.br";
    public int ScheduleHour { get; set; } = 7;
    public string ScheduleTimeZone { get; set; } = "America/Sao_Paulo";
}

public sealed class WhatsAppCredentialsResolver(
    IStoreConfigRepository storeConfigRepository,
    IOptions<WhatsAppSettings> settings) : IWhatsAppCredentialsResolver
{
    public async Task<Result<WhatsAppCredentials>> ResolveAsync(TenantId tenantId, CancellationToken ct = default)
    {
        var configResult = await storeConfigRepository.GetByTenantId(tenantId);
        if (configResult.IsFailure)
            return Result<WhatsAppCredentials>.Failure(configResult.Error);

        var config = configResult.Value;
        var phoneSource = config.Contacts;
        if (string.IsNullOrWhiteSpace(phoneSource))
            return Result<WhatsAppCredentials>.Failure("Tenant system phone is not configured.");

        var phoneResult = PhoneNumber.Create(phoneSource);
        if (phoneResult.IsFailure)
            return Result<WhatsAppCredentials>.Failure(phoneResult.Error);

        return WhatsAppCredentials.Create(
            config.ApiToken?.Value,
            settings.Value.DefaultToken,
            phoneResult.Value);
    }
}

public sealed class NotificationStoreNameAdapter(IStoreConfigRepository storeConfigRepository)
    : INotificationStoreNamePort
{
    public async Task<Result<string>> GetStoreNameAsync(TenantId tenantId, CancellationToken ct = default)
    {
        var configResult = await storeConfigRepository.GetByTenantId(tenantId);
        if (configResult.IsFailure)
            return Result<string>.Failure(configResult.Error);

        return Result<string>.Success(configResult.Value.Name.Value);
    }
}
