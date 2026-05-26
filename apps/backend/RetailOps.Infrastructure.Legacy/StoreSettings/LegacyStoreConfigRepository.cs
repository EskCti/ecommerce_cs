using RetailOps.Core.StoreSettings.Application.Ports;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.StoreSettings;

public sealed class LegacyStoreConfigRepository(IStoreSettingsLegacyPort legacyPort) : IStoreConfigRepository
{
    public async Task<Result<StoreConfig>> GetById(Guid id)
    {
        var tenantIdValue = LegacyStoreSettingsIds.ParseLegacyId(id, "0003");
        if (tenantIdValue is null)
            return Result<StoreConfig>.Failure("Invalid store config id.");

        var tenantIdResult = TenantId.Create(tenantIdValue.Value);
        if (tenantIdResult.IsFailure)
            return Result<StoreConfig>.Failure(tenantIdResult.Error);

        return await GetByTenantId(tenantIdResult.Value);
    }

    public async Task<Result<StoreConfig>> GetByTenantId(TenantId tenantId)
    {
        var result = await legacyPort.GetStoreConfigFromLegacyAsync(tenantId);
        if (result.IsFailure)
            return Result<StoreConfig>.Failure(result.Error);

        if (result.Value is null)
            return Result<StoreConfig>.Failure("Store config not found.");

        return Result<StoreConfig>.Success(result.Value);
    }

    public async Task<Result<bool>> ExistsForTenant(TenantId tenantId)
    {
        var result = await legacyPort.GetStoreConfigFromLegacyAsync(tenantId);
        if (result.IsFailure)
            return Result<bool>.Failure(result.Error);

        return Result<bool>.Success(result.Value is not null);
    }

    public async Task<Result> Save(StoreConfig entity)
    {
        var result = await legacyPort.SaveStoreConfigToLegacyAsync(entity);
        return result.IsFailure ? Result.Failure(result.Error) : Result.Success();
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Store config cannot be deleted."));
}
