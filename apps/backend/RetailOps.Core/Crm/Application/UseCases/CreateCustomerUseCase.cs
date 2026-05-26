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

        var nameResult = PersonName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(nameResult.Error);

        var cpfResult = Cpf.Create(input.Cpf);
        if (cpfResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(cpfResult.Error);

        var policyResult = await registrationPolicy.ValidateUniqueCpfAsync(
            tenantIdResult.Value,
            cpfResult.Value,
            cancellationToken: cancellationToken);
        if (policyResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(policyResult.Error);

        var contactResult = BuildOptionalContact(input.Email, input.Phone, input.Address);
        if (contactResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(contactResult.Error);

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
        Email? emailVo = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailResult = Email.Create(email);
            if (emailResult.IsFailure)
                return Result<(Email?, Phone?, Address?)>.Failure(emailResult.Error);
            emailVo = emailResult.Value;
        }

        Phone? phoneVo = null;
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var phoneResult = Phone.Create(phone);
            if (phoneResult.IsFailure)
                return Result<(Email?, Phone?, Address?)>.Failure(phoneResult.Error);
            phoneVo = phoneResult.Value;
        }

        Address? addressVo = null;
        if (!string.IsNullOrWhiteSpace(address))
        {
            var addressResult = Address.Create(address);
            if (addressResult.IsFailure)
                return Result<(Email?, Phone?, Address?)>.Failure(addressResult.Error);
            addressVo = addressResult.Value;
        }

        return Result<(Email?, Phone?, Address?)>.Success((emailVo, phoneVo, addressVo));
    }
}
