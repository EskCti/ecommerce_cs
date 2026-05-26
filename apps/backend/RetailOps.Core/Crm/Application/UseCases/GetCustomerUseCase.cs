using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.UseCases;

public sealed class GetCustomerUseCase(ICustomerRepository customerRepository)
    : IUseCase<(int tenantId, Guid customerId), CustomerOutputDto>
{
    public async Task<Result<CustomerOutputDto>> Execute(
        (int tenantId, Guid customerId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, customerId) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(tenantIdResult.Error);

        var customerResult = await customerRepository.GetById(customerId);
        if (customerResult.IsFailure)
            return Result<CustomerOutputDto>.Failure(customerResult.Error);

        var customer = customerResult.Value;
        if (customer.TenantId.Value != tenantId)
            return Result<CustomerOutputDto>.Failure("Customer does not belong to this tenant.");

        return Result<CustomerOutputDto>.Success(CustomerOutputDto.FromDomain(customer));
    }
}
