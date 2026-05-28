using RetailOps.Core.Sales.Application.Events;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Application.Ports;

public interface ISaleCompletedPublisher
{
    Task<Result> Publish(SaleCompletedEvent saleEvent, CancellationToken ct = default);
}
