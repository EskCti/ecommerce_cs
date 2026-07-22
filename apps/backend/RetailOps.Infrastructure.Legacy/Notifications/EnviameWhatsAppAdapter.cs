using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RetailOps.Core.Notifications.Application.Ports;
using RetailOps.Core.Notifications.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Infrastructure.Legacy.Notifications;

public sealed class EnviameWhatsAppAdapter(
    HttpClient httpClient,
    IOptions<WhatsAppSettings> settings,
    ILogger<EnviameWhatsAppAdapter> logger) : IWhatsAppGatewayPort
{
    public async Task<Result> SendTextAsync(
        PhoneNumber to,
        MessageTemplate body,
        WhatsAppCredentials credentials,
        CancellationToken ct = default)
    {
        var baseUrl = settings.Value.ApiBaseUrl.TrimEnd('/');
        var requestUri = $"{baseUrl}/v1/messages";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {credentials.ApiToken}");
        request.Content = JsonContent.Create(new
        {
            to = to.E164,
            message = body.Text,
            channel = "whatsapp",
        });

        try
        {
            var response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning(
                    "WhatsApp gateway returned {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    errorBody);
                return Result.Failure($"WhatsApp gateway error: {(int)response.StatusCode}");
            }

            return Result.Success();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogError(ex, "WhatsApp gateway request failed");
            return Result.Failure($"WhatsApp gateway request failed: {ex.Message}");
        }
    }
}
