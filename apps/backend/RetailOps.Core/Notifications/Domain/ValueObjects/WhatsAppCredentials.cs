using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Notifications.Domain.ValueObjects;

public sealed record WhatsAppCredentials
{
    public string ApiToken { get; }
    public PhoneNumber DestinationPhone { get; }
    public bool UsesTenantToken { get; }

    private WhatsAppCredentials(string apiToken, PhoneNumber destinationPhone, bool usesTenantToken)
    {
        ApiToken = apiToken;
        DestinationPhone = destinationPhone;
        UsesTenantToken = usesTenantToken;
    }

    public static Result<WhatsAppCredentials> Create(
        string? tenantToken,
        string? globalToken,
        PhoneNumber destinationPhone)
    {
        var token = !string.IsNullOrWhiteSpace(tenantToken) ? tenantToken.Trim() : globalToken?.Trim();
        if (string.IsNullOrWhiteSpace(token))
            return Result<WhatsAppCredentials>.Failure("WhatsApp API token is required.");

        if (token.Length < 10)
            return Result<WhatsAppCredentials>.Failure("WhatsApp API token is invalid.");

        var usesTenant = !string.IsNullOrWhiteSpace(tenantToken);
        return Result<WhatsAppCredentials>.Success(
            new WhatsAppCredentials(token, destinationPhone, usesTenant));
    }
}
