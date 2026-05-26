using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Domain.ValueObjects;

public record AttachmentPath
{
    public string Value { get; }

    private AttachmentPath(string value) => Value = value;

    public static Result<AttachmentPath> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<AttachmentPath>.Failure("Attachment path is required.");

        var trimmed = value.Trim();

        if (trimmed.Length > 500)
            return Result<AttachmentPath>.Failure("Attachment path cannot exceed 500 characters.");

        return Result<AttachmentPath>.Success(new AttachmentPath(trimmed));
    }

    public static implicit operator string(AttachmentPath path) => path.Value;
}
