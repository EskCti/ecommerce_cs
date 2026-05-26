using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Domain.ValueObjects;

public record Cpf
{
    public string Value { get; }

    private Cpf(string value) => Value = value;

    public static Result<Cpf> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Cpf>.Failure("CPF is required.");

        var cleaned = new string(value.Where(char.IsDigit).ToArray());

        if (cleaned.Length != 11)
            return Result<Cpf>.Failure("CPF must have 11 digits.");

        if (!IsValidCpf(cleaned))
            return Result<Cpf>.Failure("Invalid CPF.");

        return Result<Cpf>.Success(new Cpf(cleaned));
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.All(c => c == cpf[0]))
            return false;

        var firstDigit = CalculateCpfDigit(cpf.Substring(0, 9), 10);
        var secondDigit = CalculateCpfDigit(cpf.Substring(0, 10), 11);

        return cpf[9] == firstDigit && cpf[10] == secondDigit;
    }

    private static char CalculateCpfDigit(string baseCpf, int weightStart)
    {
        var sum = 0;
        for (var i = 0; i < baseCpf.Length; i++)
        {
            sum += int.Parse(baseCpf[i].ToString()) * (weightStart - i);
        }

        var remainder = sum % 11;
        var digit = remainder < 2 ? 0 : 11 - remainder;

        return digit.ToString()[0];
    }

    public static implicit operator string(Cpf cpf) => cpf.Value;
}
