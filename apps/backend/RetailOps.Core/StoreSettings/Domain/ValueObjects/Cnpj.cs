using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public record Cnpj
{
    public string Value { get; }

    private Cnpj(string value) => Value = value;

    public static Result<Cnpj> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Cnpj>.Failure("CNPJ is required.");

        var cleaned = new string(value.Where(char.IsDigit).ToArray());
        
        if (cleaned.Length != 14)
            return Result<Cnpj>.Failure("CNPJ must have 14 digits.");
        
        if (!IsValidCnpj(cleaned))
            return Result<Cnpj>.Failure("Invalid CNPJ.");

        return Result<Cnpj>.Success(new Cnpj(cleaned));
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Length != 14)
            return false;

        if (cnpj.All(c => c == cnpj[0]))
            return false;

        var firstDigit = CalculateCnpjDigit(cnpj.Substring(0, 12));
        var secondDigit = CalculateCnpjDigit(cnpj.Substring(0, 13));

        return cnpj[12] == firstDigit && cnpj[13] == secondDigit;
    }

    private static char CalculateCnpjDigit(string baseCnpj)
    {
        var multipliers = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        if (baseCnpj.Length == 13)
            multipliers = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var sum = 0;
        for (var i = 0; i < baseCnpj.Length; i++)
        {
            sum += int.Parse(baseCnpj[i].ToString()) * multipliers[i];
        }

        var remainder = sum % 11;
        var digit = remainder < 2 ? '0' : (11 - remainder).ToString()[0];

        return digit;
    }

    public static implicit operator string(Cnpj cnpj) => cnpj.Value;
}