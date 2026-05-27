using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.ValueObjects;

public record ProductPhotoPath
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public string Value { get; }

    private ProductPhotoPath(string value) => Value = value;

    public static Result<ProductPhotoPath> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<ProductPhotoPath>.Failure("Photo path is required.");

        var trimmed = value.Trim();
        var extension = Path.GetExtension(trimmed);

        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            return Result<ProductPhotoPath>.Failure(
                "Photo path must have a valid extension (.jpg, .jpeg, .png, .webp, .gif).");

        return Result<ProductPhotoPath>.Success(new ProductPhotoPath(trimmed));
    }

    public static implicit operator string(ProductPhotoPath path) => path.Value;
}
