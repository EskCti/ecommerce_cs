using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Domain.Services;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Application.UseCases;

public sealed class VerifyManagerPinUseCase(
    IUserRepository users,
    IPasswordHasher passwordHasher) : IUseCase<VerifyManagerPinCommand>
{
    private readonly ManagerAuthenticationService _managerAuth = new();

    public async Task<Result> Execute(VerifyManagerPinCommand input)
    {
        var user = await users.FindByIdAsync(input.UserId);
        if (user is null)
            return Result.Failure("User not found.");

        return _managerAuth.Verify(user, input.Pin, passwordHasher.Verify);
    }
}
