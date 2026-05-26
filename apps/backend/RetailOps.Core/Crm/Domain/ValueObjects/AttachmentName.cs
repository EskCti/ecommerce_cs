using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Domain.ValueObjects;

public record AttachmentName
{
    public string Value { get; }

    private AttachmentName(string value) => Value = value;

    public static Result<AttachmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<AttachmentName>.Failure("Attachment name is required.");

        var trimmed = value.Trim();

        if (trimmed.Length > 100)
            return Result<AttachmentName>.Failure("Attachment name cannot exceed 100 characters.");

        return Result<AttachmentName>.Success(new AttachmentName(trimmed));
    }

    public static implicit operator string(AttachmentName name) => name.Value;
}
