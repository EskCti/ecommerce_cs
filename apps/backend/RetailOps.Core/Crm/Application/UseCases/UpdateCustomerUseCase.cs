using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.UseCases;

public sealed class UpdateCustomerUseCase(ICustomerRepository customerRepository)
    : IUseCase<(int tenantId, Guid customerId, UpdateCustomerInputDto input), CustomerOutputDto>
{
    public async Task<Result<CustomerOutputDto>> Execute(
        (int tenantId, Guid customerId, UpdateCustomerInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, customerId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(tenantIdResult.Error);

        var customerResult = await customerRepository.GetById(customerId);
        if (customerResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(customerResult.Error);

        var customer = customerResult.Value;
        if (customer.TenantId.Value != tenantId)
            return Result<CustomerOutputDto>.Failure("Customer does not belong to this tenant.");

        PersonName? name = null;
        if (input.Name is not null)
        {
            var nameResult = PersonName.Create(input.Name);
            if (nameResult.IsFailure)
                return Result<CustomerOutputDto>.Failure(nameResult.Error);
            name = nameResult.Value;
        }

        var contactResult = CreateCustomerUseCase.BuildOptionalContact(input.Email, input.Phone, input.Address);
        if (contactResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(contactResult.Error);

        var (email, phone, address) = contactResult.Value;

        var updateResult = customer.UpdateContactInfo(
            name,
            email,
            phone,
            address,
            updateEmail: input.Email is not null,
            updatePhone: input.Phone is not null,
            updateAddress: input.Address is not null);

        if (updateResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(updateResult.Error);

        if (input.IsActive.HasValue)
        {
            var statusResult = input.IsActive.Value
                ? customer.Activate()
                : customer.Deactivate();

            if (statusResult.IsFailure)
                return Result<CustomerOutputDto>.Failure(statusResult.Error);
        }

        var saveResult = await customerRepository.Save(customer);
        if (saveResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(saveResult.Error);

        return Result<CustomerOutputDto>.Success(CustomerOutputDto.FromDomain(customer));
    }
}
