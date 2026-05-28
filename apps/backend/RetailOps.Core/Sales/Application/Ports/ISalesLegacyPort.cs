using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.Ports;

public interface ISalesLegacyPort
{
    Task<Result<int>> OpenSession(CashSession session, CancellationToken ct = default);
    Task<Result> SaveSession(CashSession session, CancellationToken ct = default);
    Task<Result<int>> AddCartLine(CashSession session, SaleLine line, CancellationToken ct = default);
    Task<Result> RemoveCartLine(CashSession session, Guid lineId, CancellationToken ct = default);
    Task<Result<int>> FinalizeSale(CashSession session, Sale sale, CancellationToken ct = default);
    Task<Result> CloseSession(CashSession session, CancellationToken ct = default);
    Task<Result> CancelSale(Sale sale, CancellationToken ct = default);
}
