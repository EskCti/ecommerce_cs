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

public sealed class LegacySupplierRepository(
    ICrmLegacyPort legacyPort,
    LegacySasDbContext db) : ISupplierRepository
{
    public async Task<Result<Supplier>> GetById(Guid id)
    {
        var legacyId = LegacyCrmIds.ParseLegacyId(id, "0007");
        if (legacyId is null)
            return Result<Supplier>.Failure("Invalid supplier id.");

        var row = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == legacyId);

        if (row is null)
            return Result<Supplier>.Failure("Supplier not found.");

        var mapped = LegacySupplierMapper.ToDomain(row);
        return mapped.IsFailure
            ? Result<Supplier>.Failure(mapped.Error)
            : Result<Supplier>.Success(mapped.Value);
    }

    public async Task<Result<IEnumerable<Supplier>>> GetByTenantId(TenantId tenantId)
    {
        var result = await legacyPort.GetSuppliersFromLegacyAsync(tenantId);
        return result.IsFailure
            ? Result<IEnumerable<Supplier>>.Failure(result.Error)
            : Result<IEnumerable<Supplier>>.Success(result.Value);
    }

    public async Task<Result<bool>> TaxDocumentExists(
        TenantId tenantId,
        TaxDocument taxDocument,
        Guid? excludeSupplierId = null)
    {
        var listResult = await GetByTenantId(tenantId);
        if (listResult.IsFailure)
            return Result<bool>.Failure(listResult.Error);

        var exists = listResult.Value.Any(s =>
            s.TaxDocument.Value == taxDocument.Value
            && (excludeSupplierId is null || s.Id != excludeSupplierId));

        return Result<bool>.Success(exists);
    }

    public async Task<Result> Save(Supplier entity)
    {
        var result = await legacyPort.SaveSupplierToLegacyAsync(entity);
        return result.IsFailure ? Result.Failure(result.Error) : Result.Success();
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Supplier cannot be deleted. Use deactivation instead."));
}
