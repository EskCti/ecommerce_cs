using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Infrastructure.Legacy.Mappers.Catalog;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Catalog;

public sealed class LegacyCategoryRepository(
    ICatalogLegacyPort legacyPort,
    LegacySasDbContext db) : ICategoryRepository
{
    public async Task<Result<Category>> GetById(Guid id)
    {
        var legacyId = LegacyCatalogIds.ParseLegacyId(id, "0010");
        if (legacyId is null)
            return Result<Category>.Failure("Invalid category id.");

        var row = await db.Categories.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == legacyId);

        if (row is null)
            return Result<Category>.Failure("Category not found.");

        var mapped = LegacyCategoryMapper.ToDomain(row);
        return mapped.IsFailure
            ? Result<Category>.Failure(mapped.Error)
            : Result<Category>.Success(mapped.Value);
    }

    public async Task<Result<IEnumerable<Category>>> GetByTenantId(TenantId tenantId)
    {
        var result = await legacyPort.GetCategoriesFromLegacyAsync(tenantId);
        return result.IsFailure
            ? Result<IEnumerable<Category>>.Failure(result.Error)
            : Result<IEnumerable<Category>>.Success(result.Value);
    }

    public async Task<Result> Save(Category entity)
    {
        var result = await legacyPort.SaveCategoryToLegacyAsync(entity);
        return result.IsFailure ? Result.Failure(result.Error) : Result.Success();
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Category cannot be deleted. Use deactivation instead."));
}
