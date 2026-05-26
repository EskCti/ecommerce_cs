using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Crm;

public static class LegacySupplierMapper
{
    public static Result<Supplier> ToDomain(LegacySupplierRow row)
    {
        try
        {
            var tenantIdResult = TenantId.Create(row.CompanyId);
            if (tenantIdResult.IsFailure)
                return Result<Supplier>.Failure(tenantIdResult.Error);

            if (string.IsNullOrWhiteSpace(row.Nome))
                return Result<Supplier>.Failure("Supplier name is required.");

            if (string.IsNullOrWhiteSpace(row.Cpf))
                return Result<Supplier>.Failure("Supplier tax document is required.");

            var nameResult = PersonName.Create(row.Nome);
            if (nameResult.IsFailure)
                return Result<Supplier>.Failure(nameResult.Error);

            var personType = ParsePersonType(row.Pessoa);
            var taxDocumentResult = TaxDocument.Create(row.Cpf, personType);
            if (taxDocumentResult.IsFailure)
                return Result<Supplier>.Failure(taxDocumentResult.Error);

            Email? email = null;
            if (!string.IsNullOrWhiteSpace(row.Email))
            {
                var emailResult = Email.Create(row.Email);
                if (emailResult.IsFailure)
                    return Result<Supplier>.Failure(emailResult.Error);
                email = emailResult.Value;
            }

            Phone? phone = null;
            if (!string.IsNullOrWhiteSpace(row.Telefone))
            {
                var phoneResult = Phone.Create(row.Telefone);
                if (phoneResult.IsFailure)
                    return Result<Supplier>.Failure(phoneResult.Error);
                phone = phoneResult.Value;
            }

            Address? address = null;
            if (!string.IsNullOrWhiteSpace(row.Endereco))
            {
                var addressResult = Address.Create(row.Endereco);
                if (addressResult.IsFailure)
                    return Result<Supplier>.Failure(addressResult.Error);
                address = addressResult.Value;
            }

            var isActive = row.Ativo is null
                || row.Ativo.Equals("S", StringComparison.OrdinalIgnoreCase)
                || row.Ativo.Equals("Sim", StringComparison.OrdinalIgnoreCase);

            return Supplier.Reconstitute(
                LegacyCrmIds.Supplier(row.Id),
                tenantIdResult.Value,
                nameResult.Value,
                personType,
                taxDocumentResult.Value,
                email,
                phone,
                address,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            return Result<Supplier>.Failure($"Failed to map legacy Supplier: {ex.Message}");
        }
    }

    public static Result<LegacySupplierRow> ToLegacy(Supplier supplier, int? legacyId = null)
    {
        try
        {
            var row = new LegacySupplierRow
            {
                Id = legacyId ?? LegacyCrmIds.ParseLegacyId(supplier.Id, "0007") ?? 0,
                CompanyId = supplier.TenantId.Value,
                Nome = supplier.Name.Value,
                Pessoa = ToLegacyPersonType(supplier.PersonType),
                Cpf = supplier.TaxDocument.Value,
                Telefone = supplier.Phone?.Value,
                Email = supplier.Email?.Value,
                Endereco = supplier.Address?.Value,
                Ativo = supplier.IsActive ? "Sim" : "Não"
            };

            return Result<LegacySupplierRow>.Success(row);
        }
        catch (Exception ex)
        {
            return Result<LegacySupplierRow>.Failure($"Failed to map Supplier to legacy: {ex.Message}");
        }
    }

    private static PersonType ParsePersonType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return PersonType.Individual;

        var normalized = value.Trim().ToUpperInvariant();
        return normalized is "J" or "JURIDICA" or "JURÍDICA" or "COMPANY"
            ? PersonType.Company
            : PersonType.Individual;
    }

    private static string ToLegacyPersonType(PersonType personType) =>
        personType == PersonType.Company ? "J" : "F";
}
