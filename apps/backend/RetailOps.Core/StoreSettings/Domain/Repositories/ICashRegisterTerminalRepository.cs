using RetailOps.Shared.Kernel.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Domain.Repositories;

public interface ICashRegisterTerminalRepository : IRepository<CashRegisterTerminal>
{
    Task<Result<IEnumerable<CashRegisterTerminal>>> GetByTenantId(TenantId tenantId);
    Task<Result<IEnumerable<CashRegisterTerminal>>> GetByStatus(TenantId tenantId, TerminalStatus status);
    Task<Result<CashRegisterTerminal>> GetByName(TenantId tenantId, CashRegisterTerminalName name);
}