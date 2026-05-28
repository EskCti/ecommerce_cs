using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Sales;

/// <summary>
/// Development stub: accepts any non-empty PIN. Replace with Identity-backed verifier in production.
/// </summary>
public sealed class ManagerPinVerifierStub : IManagerPinVerifier
{
    public Task<Result> VerifyAsync(
        TenantId tenantId,
        Guid managerUserId,
        string pin,
        CancellationToken ct = default)
    {
        if (managerUserId == Guid.Empty)
            return Task.FromResult(Result.Failure("Manager user id is required."));

        if (string.IsNullOrWhiteSpace(pin))
            return Task.FromResult(Result.Failure("Manager PIN is required."));

        return Task.FromResult(Result.Success());
    }
}
