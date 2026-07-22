using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Returns.Domain.ValueObjects;

public record ExchangeId
{
    public Guid Value { get; }

    private ExchangeId(Guid value) => Value = value;

    public static Result<ExchangeId> Create(Guid value)
    {
        if (value == Guid.Empty)
            return Result<ExchangeId>.Failure("Exchange id is required.");

        return Result<ExchangeId>.Success(new ExchangeId(value));
    }

    public static implicit operator Guid(ExchangeId id) => id.Value;
}
