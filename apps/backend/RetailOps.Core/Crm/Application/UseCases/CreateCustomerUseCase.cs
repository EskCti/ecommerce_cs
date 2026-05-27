using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.Services;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.UseCases;

public sealed class CreateCustomerUseCase(
    ICustomerRepository customerRepository,
    CustomerRegistrationPolicy registrationPolicy) : IUseCase<(int tenantId, CreateCustomerInputDto input), CustomerOutputDto>
{
    public async Task<Result<CustomerOutputDto>> Execute(
        (int tenantId, CreateCustomerInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(tenantIdResult.Error);

        var validationErrors = new List<string>();

        var nameResult = PersonName.Create(input.Name);
        if (nameResult.IsFailure)
            validationErrors.Add(nameResult.Error);

        var cpfResult = Cpf.Create(input.Cpf);
        if (cpfResult.IsFailure)
            validationErrors.Add(cpfResult.Error);

        var contactResult = BuildOptionalContact(input.Email, input.Phone, input.Address);
        if (contactResult.IsFailure)
            validationErrors.Add(contactResult.Error);

        if (validationErrors.Count > 0)
            return Result<CustomerOutputDto>.Failure(string.Join(" • ", validationErrors));

        if (nameResult.IsFailure || cpfResult.IsFailure || contactResult.IsFailure)
            return Result<CustomerOutputDto>.Failure("Validation failed.");

        var policyResult = await registrationPolicy.ValidateUniqueCpfAsync(
            tenantIdResult.Value,
            cpfResult.Value,
            cancellationToken: cancellationToken);
        if (policyResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(policyResult.Error);

        var (email, phone, address) = contactResult.Value;

        var customerResult = Customer.Create(
            tenantIdResult.Value,
            nameResult.Value,
            cpfResult.Value,
            email,
            phone,
            address,
            input.IsActive);

        if (customerResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(customerResult.Error);

        var saveResult = await customerRepository.Save(customerResult.Value);
        if (saveResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(saveResult.Error);

        return Result<CustomerOutputDto>.Success(CustomerOutputDto.FromDomain(customerResult.Value));
    }

    internal static Result<(Email? Email, Phone? Phone, Address? Address)> BuildOptionalContact(
        string? email,
        string? phone,
        string? address)
    {
        var errors = new List<string>();
        Email? emailVo = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailResult = Email.Create(email);
            if (emailResult.IsFailure)
                errors.Add(emailResult.Error);
            else
                emailVo = emailResult.Value;
        }

        Phone? phoneVo = null;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var phoneResult = Phone.Create(phone);
            if (phoneResult.IsFailure)
                errors.Add(phoneResult.Error);
            else
                phoneVo = phoneResult.Value;
        }

        Address? addressVo = null;
        if (!string.IsNullOrWhiteSpace(address))
        {
            var addressResult = Address.Create(address);
            if (addressResult.IsFailure)
                errors.Add(addressResult.Error);
            else
                addressVo = addressResult.Value;
        }

        if (errors.Count > 0)
            return Result<(Email?, Phone?, Address?)>.Failure(string.Join(" • ", errors));

        return Result<(Email?, Phone?, Address?)>.Success((emailVo, phoneVo, addressVo));
    }
}
