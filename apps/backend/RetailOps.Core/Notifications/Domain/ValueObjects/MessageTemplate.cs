using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Notifications.Domain.ValueObjects;

public sealed record MessageTemplate
{
    public const int MaxLength = 4096;

    public string Text { get; }

    private MessageTemplate(string text) => Text = text;

    public static Result<MessageTemplate> Create(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result<MessageTemplate>.Failure("Message text is required.");

        var trimmed = text.Trim();
        if (trimmed.Length > MaxLength)
            return Result<MessageTemplate>.Failure($"Message exceeds maximum length of {MaxLength} characters.");

        return Result<MessageTemplate>.Success(new MessageTemplate(trimmed));
    }
}
