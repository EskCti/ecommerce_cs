using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public record ImagePath
{
    public string Value { get; }

    private ImagePath(string value) => Value = value;

    public static Result<ImagePath> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<ImagePath>.Failure("Image path is required.");

        var trimmed = value.Trim();
        
        if (trimmed.Length > 500)
            return Result<ImagePath>.Failure("Image path cannot exceed 500 characters.");

        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" };
        var extension = Path.GetExtension(trimmed).ToLowerInvariant();
        
        if (!validExtensions.Contains(extension))
            return Result<ImagePath>.Failure($"Invalid image extension. Valid extensions are: {string.Join(", ", validExtensions)}.");

        return Result<ImagePath>.Success(new ImagePath(trimmed));
    }

    public static implicit operator string(ImagePath imagePath) => imagePath.Value;
}