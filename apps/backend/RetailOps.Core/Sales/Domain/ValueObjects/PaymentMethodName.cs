using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.ValueObjects;

public record PaymentMethodName
{
    public string Value { get; }

    private PaymentMethodName(string value) => Value = value;

    public static Result<PaymentMethodName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<PaymentMethodName>.Failure("Payment method name is required.");

        var trimmed = value.Trim();
        if (trimmed.Length > 50)
            return Result<PaymentMethodName>.Failure("Payment method name cannot exceed 50 characters.");

        return Result<PaymentMethodName>.Success(new PaymentMethodName(trimmed));
    }
}
