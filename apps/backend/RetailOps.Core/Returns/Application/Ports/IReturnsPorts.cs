using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Returns.Application.Ports;

public interface ICustomerProvisioningPort
{
    Task<Result<Guid>> FindOrCreateByCpfAsync(
        TenantId tenantId,
        string cpf,
        string name,
        CancellationToken ct = default);
}

public interface IReturnsLegacyPort
{
    Task<Result<int>> SaveExchangeToLegacyAsync(
        Domain.Entities.Exchange exchange,
        CancellationToken ct = default);

    Task<Result> DeleteExchangeFromLegacyAsync(
        TenantId tenantId,
        Guid exchangeId,
        CancellationToken ct = default);
}
