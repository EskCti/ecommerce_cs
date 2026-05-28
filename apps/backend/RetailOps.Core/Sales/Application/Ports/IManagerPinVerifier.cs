using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.Ports;

public interface IManagerPinVerifier
{
    Task<Result> VerifyAsync(TenantId tenantId, Guid managerUserId, string pin, CancellationToken ct = default);
}
