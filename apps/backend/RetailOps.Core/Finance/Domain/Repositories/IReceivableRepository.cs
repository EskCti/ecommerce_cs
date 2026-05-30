using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Finance.Domain.Repositories;

public interface IReceivableRepository
{
    Task<Result<Receivable>> GetById(Guid id);
    Task<Result<Receivable?>> GetBySaleId(Guid saleId);
    Task<Result> Save(Receivable entity);
    Task<Result> Delete(Guid id);
}
