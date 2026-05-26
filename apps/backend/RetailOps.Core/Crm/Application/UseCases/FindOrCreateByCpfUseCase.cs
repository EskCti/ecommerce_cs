using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Application.Events;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.Services;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.UseCases;

public sealed class FindOrCreateByCpfUseCase(
    ICustomerRepository customerRepository,
    CustomerRegistrationPolicy registrationPolicy) : IUseCase<(int tenantId, FindOrCreateByCpfInputDto input), FindOrCreateByCpfOutputDto>
{
    public CustomerRegistered? LastRegisteredEvent { get; private set; }
    public CustomerFoundByCpf? LastFoundEvent { get; private set; }

    public async Task<Result<FindOrCreateByCpfOutputDto>> Execute(
        (int tenantId, FindOrCreateByCpfInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, input) = request;
        LastRegisteredEvent = null;
        LastFoundEvent = null;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<FindOrCreateByCpfOutputDto>.Failure(tenantIdResult.Error);

        var cpfResult = Cpf.Create(input.Cpf);
        if (cpfResult.IsFailure)
            return Result<FindOrCreateByCpfOutputDto>.Failure(cpfResult.Error);

        var existingResult = await customerRepository.GetByCpf(tenantIdResult.Value, cpfResult.Value);
        if (existingResult.IsFailure)
            return Result<FindOrCreateByCpfOutputDto>.Failure(existingResult.Error);

        if (existingResult.Value is not null)
        {
            LastFoundEvent = new CustomerFoundByCpf(
                existingResult.Value.Id,
                tenantId,
                cpfResult.Value.Value);

            return Result<FindOrCreateByCpfOutputDto>.Success(new FindOrCreateByCpfOutputDto
            {
                Customer = CustomerOutputDto.FromDomain(existingResult.Value),
                Created = false
            });
        }

        var nameResult = PersonName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<FindOrCreateByCpfOutputDto>.Failure(nameResult.Error);

        var policyResult = await registrationPolicy.ValidateUniqueCpfAsync(
            tenantIdResult.Value,
            cpfResult.Value,
            cancellationToken: cancellationToken);
        if (policyResult.IsFailure)
            return Result<FindOrCreateByCpfOutputDto>.Failure(policyResult.Error);

        var contactResult = CreateCustomerUseCase.BuildOptionalContact(input.Email, input.Phone, input.Address);
        if (contactResult.IsFailure)
            return Result<FindOrCreateByCpfOutputDto>.Failure(contactResult.Error);

        var (email, phone, address) = contactResult.Value;

        var customerResult = Customer.Create(
            tenantIdResult.Value,
            nameResult.Value,
            cpfResult.Value,
            email,
            phone,
            address);

        if (customerResult.IsFailure)
            return Result<FindOrCreateByCpfOutputDto>.Failure(customerResult.Error);

        var saveResult = await customerRepository.Save(customerResult.Value);
        if (saveResult.IsFailure)
            return Result<FindOrCreateByCpfOutputDto>.Failure(saveResult.Error);

        LastRegisteredEvent = new CustomerRegistered(
            customerResult.Value.Id,
            tenantId,
            cpfResult.Value.Value);

        return Result<FindOrCreateByCpfOutputDto>.Success(new FindOrCreateByCpfOutputDto
        {
            Customer = CustomerOutputDto.FromDomain(customerResult.Value),
            Created = true
        });
    }
}
