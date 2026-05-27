using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.ValueObjects;

public record CategoryId
{
    public Guid Value { get; }

    private CategoryId(Guid value) => Value = value;

    public static Result<CategoryId> Create(Guid value)
    {
        if (value == Guid.Empty)
            return Result<CategoryId>.Failure("Category id is required.");

        return Result<CategoryId>.Success(new CategoryId(value));
    }

    public static implicit operator Guid(CategoryId id) => id.Value;
}
