using RetailOps.Core.Sales.Application.Events;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Infrastructure.Legacy.Sales;

public sealed class SaleCompletedPublisherStub : ISaleCompletedPublisher
{
    public Task<Result> Publish(SaleCompletedEvent saleEvent, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}
