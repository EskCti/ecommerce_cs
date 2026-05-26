using Microsoft.EntityFrameworkCore;
using RetailOps.Core.StoreSettings.Application.Ports;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.StoreSettings;

public sealed class LegacyPaymentMethodRepository(
    IStoreSettingsLegacyPort legacyPort,
    LegacySasDbContext db) : IPaymentMethodRepository
{
    public async Task<Result<PaymentMethod>> GetById(Guid id)
    {
        var legacyId = LegacyStoreSettingsIds.ParseLegacyId(id, "0004");
        if (legacyId is null)
            return Result<PaymentMethod>.Failure("Invalid payment method id.");

        var row = await db.PaymentMethods.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == legacyId);

        if (row is null)
            return Result<PaymentMethod>.Failure("Payment method not found.");

        var mapped = LegacyPaymentMethodMapper.ToDomain(row);
        return mapped.IsFailure
            ? Result<PaymentMethod>.Failure(mapped.Error)
            : Result<PaymentMethod>.Success(mapped.Value);
    }

    public async Task<Result<IEnumerable<PaymentMethod>>> GetByTenantId(TenantId tenantId)
    {
        var result = await legacyPort.GetPaymentMethodsFromLegacyAsync(tenantId);
        if (result.IsFailure)
            return Result<IEnumerable<PaymentMethod>>.Failure(result.Error);

        return Result<IEnumerable<PaymentMethod>>.Success(result.Value);
    }

    public async Task<Result<PaymentMethod>> GetByName(TenantId tenantId, PaymentMethodName name)
    {
        var listResult = await GetByTenantId(tenantId);
        if (listResult.IsFailure)
            return Result<PaymentMethod>.Failure(listResult.Error);

        var match = listResult.Value.FirstOrDefault(p =>
            p.Name.Value.Equals(name.Value, StringComparison.OrdinalIgnoreCase));

        return match is null
            ? Result<PaymentMethod>.Failure("Payment method not found.")
            : Result<PaymentMethod>.Success(match);
    }

    public async Task<Result<bool>> NameExists(TenantId tenantId, PaymentMethodName name, Guid? excludeId = null)
    {
        var listResult = await GetByTenantId(tenantId);
        if (listResult.IsFailure)
            return Result<bool>.Failure(listResult.Error);

        var exists = listResult.Value.Any(p =>
            p.Name.Value.Equals(name.Value, StringComparison.OrdinalIgnoreCase)
            && (excludeId is null || p.Id != excludeId));

        return Result<bool>.Success(exists);
    }

    public async Task<Result> Save(PaymentMethod entity)
    {
        var result = await legacyPort.SavePaymentMethodToLegacyAsync(entity);
        return result.IsFailure ? Result.Failure(result.Error) : Result.Success();
    }

    public async Task<Result> Delete(Guid id)
    {
        var paymentMethodResult = await GetById(id);
        if (paymentMethodResult.IsFailure)
            return Result.Failure(paymentMethodResult.Error);

        return await legacyPort.DeletePaymentMethodFromLegacyAsync(
            paymentMethodResult.Value.TenantId,
            id);
    }
}
