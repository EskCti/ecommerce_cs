using RetailOps.Core.Crm.Application.Ports;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.Crm;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace RetailOps.Infrastructure.Legacy.Crm;

public sealed class LegacyCustomerRepository(
    ICrmLegacyPort legacyPort,
    LegacySasDbContext db) : ICustomerRepository
{
    public async Task<Result<Customer>> GetById(Guid id)
    {
        var legacyId = LegacyCrmIds.ParseLegacyId(id, "0006");
        if (legacyId is null)
            return Result<Customer>.Failure("Invalid customer id.");

        var row = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == legacyId);

        if (row is null)
            return Result<Customer>.Failure("Customer not found.");

        var customerId = LegacyCrmIds.Customer(row.Id);
        var tenantId = TenantId.Create(row.CompanyId);
        if (tenantId.IsFailure)
            return Result<Customer>.Failure(tenantId.Error);

        var attachmentsResult = await legacyPort.GetCustomerAttachmentsFromLegacyAsync(tenantId.Value, customerId);
        if (attachmentsResult.IsFailure)
            return Result<Customer>.Failure(attachmentsResult.Error);

        var mapped = LegacyCustomerMapper.ToDomain(row, attachmentsResult.Value);
        return mapped.IsFailure
            ? Result<Customer>.Failure(mapped.Error)
            : Result<Customer>.Success(mapped.Value);
    }

    public async Task<Result<Customer?>> GetByCpf(TenantId tenantId, Cpf cpf)
    {
        var result = await legacyPort.GetCustomerByCpfFromLegacyAsync(tenantId, cpf.Value);
        return result.IsFailure
            ? Result<Customer?>.Failure(result.Error)
            : Result<Customer?>.Success(result.Value);
    }

    public async Task<Result<bool>> CpfExists(TenantId tenantId, Cpf cpf, Guid? excludeCustomerId = null)
    {
        var existingResult = await GetByCpf(tenantId, cpf);
        if (existingResult.IsFailure)
            return Result<bool>.Failure(existingResult.Error);

        if (existingResult.Value is null)
            return Result<bool>.Success(false);

        if (excludeCustomerId is not null && existingResult.Value.Id == excludeCustomerId)
            return Result<bool>.Success(false);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<Customer>>> GetByTenantId(TenantId tenantId)
    {
        var result = await legacyPort.GetCustomersFromLegacyAsync(tenantId);
        return result.IsFailure
            ? Result<IEnumerable<Customer>>.Failure(result.Error)
            : Result<IEnumerable<Customer>>.Success(result.Value);
    }

    public async Task<Result> Save(Customer entity)
    {
        var result = await legacyPort.SaveCustomerToLegacyAsync(entity);
        return result.IsFailure ? Result.Failure(result.Error) : Result.Success();
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Customer cannot be deleted. Use deactivation instead."));
}
