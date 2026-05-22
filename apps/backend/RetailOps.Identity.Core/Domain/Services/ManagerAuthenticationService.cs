using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Domain.Services;

public sealed class ManagerAuthenticationService
{
    public Result Verify(User user, string plainPin, Func<string, string, bool> bcryptVerify)
    {
        if (user.Level is not (UserLevel.Gerente or UserLevel.Administrador or UserLevel.Sas))
            return Result.Failure("User level cannot verify manager PIN.");

        if (user.ManagerPinHash is null)
            return Result.Failure("Manager PIN not configured.");

        var ok = bcryptVerify(plainPin, user.ManagerPinHash.Value.BcryptHash);
        return ok ? Result.Success() : Result.Failure("Invalid manager PIN.");
    }
}
