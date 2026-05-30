using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Finance.Domain.Repositories;

public interface IPayableRepository
{
    Task<Result<Payable>> GetById(Guid id);
    Task<Result> Save(Payable entity);
    Task<Result> Delete(Guid id);
}
