using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Domain.ValueObjects;

public record TaxDocument
{
    public string Value { get; }
    public PersonType PersonType { get; }

    private TaxDocument(string value, PersonType personType)
    {
        Value = value;
        PersonType = personType;
    }

    public static Result<TaxDocument> Create(string value, PersonType personType)
    {
        return personType switch
        {
            PersonType.Individual => CreateFromCpf(value),
            PersonType.Company => CreateFromCnpj(value),
            _ => Result<TaxDocument>.Failure("Invalid person type.")
        };
    }

    private static Result<TaxDocument> CreateFromCpf(string value)
    {
        var cpfResult = Cpf.Create(value);
        return cpfResult.IsFailure
            ? Result<TaxDocument>.Failure(cpfResult.Error)
            : Result<TaxDocument>.Success(new TaxDocument(cpfResult.Value.Value, PersonType.Individual));
    }

    private static Result<TaxDocument> CreateFromCnpj(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<TaxDocument>.Failure("CNPJ is required.");

        var cleaned = new string(value.Where(char.IsDigit).ToArray());

        if (cleaned.Length != 14)
            return Result<TaxDocument>.Failure("CNPJ must have 14 digits.");

        if (!IsValidCnpj(cleaned))
            return Result<TaxDocument>.Failure("Invalid CNPJ.");

        return Result<TaxDocument>.Success(new TaxDocument(cleaned, PersonType.Company));
    }

    private static bool IsValidCnpj(string cnpj)
    {
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

    public static implicit operator string(TaxDocument document) => document.Value;
}
