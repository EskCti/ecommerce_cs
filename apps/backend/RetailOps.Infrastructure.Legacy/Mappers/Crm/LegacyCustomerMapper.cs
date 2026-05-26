using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Crm;

public static class LegacyCustomerMapper
{
    public const string AttachmentType = "Cliente";

    public static Result<Customer> ToDomain(LegacyCustomerRow row, IEnumerable<CustomerAttachment>? attachments = null)
    {
        try
        {
            var tenantIdResult = TenantId.Create(row.CompanyId);
            if (tenantIdResult.IsFailure)
                return Result<Customer>.Failure(tenantIdResult.Error);

            if (string.IsNullOrWhiteSpace(row.Nome))
                return Result<Customer>.Failure("Customer name is required.");

            if (string.IsNullOrWhiteSpace(row.Cpf))
                return Result<Customer>.Failure("Customer CPF is required.");

            var nameResult = PersonName.Create(row.Nome);
            if (nameResult.IsFailure)
                return Result<Customer>.Failure(nameResult.Error);

            var cpfResult = Cpf.Create(row.Cpf);
            if (cpfResult.IsFailure)
                return Result<Customer>.Failure(cpfResult.Error);

            Email? email = null;
            if (!string.IsNullOrWhiteSpace(row.Email))
            {
                var emailResult = Email.Create(row.Email);
                if (emailResult.IsFailure)
                    return Result<Customer>.Failure(emailResult.Error);
                email = emailResult.Value;
            }

            Phone? phone = null;
            if (!string.IsNullOrWhiteSpace(row.Telefone))
            {
                var phoneResult = Phone.Create(row.Telefone);
                if (phoneResult.IsFailure)
                    return Result<Customer>.Failure(phoneResult.Error);
                phone = phoneResult.Value;
            }

            Address? address = null;
            if (!string.IsNullOrWhiteSpace(row.Endereco))
            {
                var addressResult = Address.Create(row.Endereco);
                if (addressResult.IsFailure)
                    return Result<Customer>.Failure(addressResult.Error);
                address = addressResult.Value;
            }

            var isActive = row.Ativo is null
                || row.Ativo.Equals("S", StringComparison.OrdinalIgnoreCase)
                || row.Ativo.Equals("Sim", StringComparison.OrdinalIgnoreCase);

            return Customer.Reconstitute(
                LegacyCrmIds.Customer(row.Id),
                tenantIdResult.Value,
                nameResult.Value,
                cpfResult.Value,
                email,
                phone,
                address,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow,
                attachments);
        }
        catch (Exception ex)
        {
            return Result<Customer>.Failure($"Failed to map legacy Customer: {ex.Message}");
        }
    }

    public static Result<LegacyCustomerRow> ToLegacy(Customer customer, int? legacyId = null)
    {
        try
        {
            var row = new LegacyCustomerRow
            {
                Id = legacyId ?? LegacyCrmIds.ParseLegacyId(customer.Id, "0006") ?? 0,
                CompanyId = customer.TenantId.Value,
                Nome = customer.Name.Value,
                Cpf = customer.Cpf.Value,
                Telefone = customer.Phone?.Value,
                Email = customer.Email?.Value,
                Endereco = customer.Address?.Value,
                Ativo = customer.IsActive ? "Sim" : "Não"
            };

            return Result<LegacyCustomerRow>.Success(row);
        }
        catch (Exception ex)
        {
            return Result<LegacyCustomerRow>.Failure($"Failed to map Customer to legacy: {ex.Message}");
        }
    }
}
