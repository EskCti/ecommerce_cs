using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Domain.ValueObjects;

public readonly record struct Cpf(string Value)
{
    public static Result<Cpf> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Cpf>.Failure("CPF is required.");

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 11)
            return Result<Cpf>.Failure("CPF must have 11 digits.");

        return Result<Cpf>.Success(new Cpf(digits));
    }
}
