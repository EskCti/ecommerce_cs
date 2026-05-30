using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Finance.Domain.Repositories;

public interface ICommissionRepository
{
    Task<Result<Commission>> GetById(Guid id);
    Task<Result<Commission?>> GetBySaleId(Guid saleId);
    Task<Result<IReadOnlyList<Commission>>> GetByIds(IReadOnlyList<Guid> ids);
    Task<Result> Save(Commission entity);
    Task<Result> DeleteBySaleId(Guid saleId);
}
