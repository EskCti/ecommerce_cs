using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Domain.Services;

public sealed class CustomerRegistrationPolicy(ICustomerRepository customerRepository)
{
    public async Task<Result> ValidateUniqueCpfAsync(
        TenantId tenantId,
        Cpf cpf,
        Guid? excludeCustomerId = null,
        CancellationToken cancellationToken = default)
    {
        var existsResult = await customerRepository.CpfExists(tenantId, cpf, excludeCustomerId);
        if (existsResult.IsFailure)
            return Result.Failure(existsResult.Error);

        if (existsResult.Value)
            return Result.Failure($"CPF '{cpf.Value}' is already registered for this tenant.");

        return Result.Success();
    }
}
