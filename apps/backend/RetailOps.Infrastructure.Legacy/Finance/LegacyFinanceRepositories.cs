using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Infrastructure.Legacy.Finance;

public sealed class LegacyReceivableRepository(FinanceLegacyAdapter legacyPort) : IReceivableRepository
{
    public Task<Result<Receivable>> GetById(Guid id) => legacyPort.GetReceivableByIdAsync(id);

    public Task<Result<Receivable?>> GetBySaleId(Guid saleId) => legacyPort.GetReceivableBySaleIdAsync(saleId);

    public Task<Result> Save(Receivable entity) => legacyPort.SaveManualReceivableToLegacyAsync(entity);

    public async Task<Result> Delete(Guid id)
    {
        var receivable = await GetById(id);
        if (receivable.IsFailure)
            return Result.Failure(receivable.Error);

        return await legacyPort.DeleteManualReceivableFromLegacyAsync(receivable.Value.TenantId, id);
    }
}

public sealed class LegacyPayableRepository(FinanceLegacyAdapter legacyPort) : IPayableRepository
{
    public Task<Result<Payable>> GetById(Guid id) => legacyPort.GetPayableByIdAsync(id);

    public Task<Result> Save(Payable entity) => legacyPort.SavePayableToLegacyAsync(entity);

    public async Task<Result> Delete(Guid id)
    {
        var payable = await GetById(id);
        if (payable.IsFailure)
            return Result.Failure(payable.Error);

        return await legacyPort.DeletePayableFromLegacyAsync(payable.Value.TenantId, id);
    }
}

public sealed class LegacyCommissionRepository(FinanceLegacyAdapter legacyPort) : ICommissionRepository
{
    public Task<Result<Commission>> GetById(Guid id) => legacyPort.GetCommissionByIdAsync(id);

    public Task<Result<Commission?>> GetBySaleId(Guid saleId) => legacyPort.GetCommissionBySaleIdAsync(saleId);

    public async Task<Result<IReadOnlyList<Commission>>> GetByIds(IReadOnlyList<Guid> ids)
    {
        var items = new List<Commission>();
        foreach (var id in ids)
        {
            var result = await GetById(id);
            if (result.IsFailure)
                return Result<IReadOnlyList<Commission>>.Failure(result.Error);
            items.Add(result.Value);
        }

        return Result<IReadOnlyList<Commission>>.Success(items);
    }

    public Task<Result> Save(Commission entity) => legacyPort.SaveCommissionToLegacyAsync(entity);

    public async Task<Result> DeleteBySaleId(Guid saleId)
    {
        var commission = await GetBySaleId(saleId);
        if (commission.IsFailure)
            return Result.Failure(commission.Error);

        if (commission.Value is null)
            return Result.Success();

        return await legacyPort.DeleteCommissionBySaleFromLegacyAsync(commission.Value.TenantId, saleId);
    }
}
