using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Domain.ValueObjects;

public record SupplierId
{
    public Guid Value { get; }

    private SupplierId(Guid value) => Value = value;

    public static Result<SupplierId> Create(Guid value)
    {
        if (value == Guid.Empty)
            return Result<SupplierId>.Failure("Supplier id is required.");

        return Result<SupplierId>.Success(new SupplierId(value));
    }

    public static implicit operator Guid(SupplierId id) => id.Value;
}
