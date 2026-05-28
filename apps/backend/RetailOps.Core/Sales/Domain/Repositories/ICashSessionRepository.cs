using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Domain.Repositories;

public interface ICashSessionRepository : IRepository<CashSession>
{
    Task<Result<CashSession?>> GetOpenByOperator(TenantId tenantId, Guid operatorUserId);
    Task<Result<CashSession?>> GetOpenByTerminal(TenantId tenantId, Guid terminalId);
}
