using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Finance.Domain.ValueObjects;

public record AttachmentMeta
{
    public string Name { get; }
    public string Path { get; }

    private AttachmentMeta(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public static Result<AttachmentMeta> Create(string name, string path)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<AttachmentMeta>.Failure("Attachment name is required.");

        if (string.IsNullOrWhiteSpace(path))
            return Result<AttachmentMeta>.Failure("Attachment path is required.");

        return Result<AttachmentMeta>.Success(new AttachmentMeta(name.Trim(), path.Trim()));
    }
}
