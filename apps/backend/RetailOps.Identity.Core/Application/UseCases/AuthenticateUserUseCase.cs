using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Domain.Services;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Application.UseCases;

public sealed class AuthenticateUserUseCase(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwt) : IUseCase<AuthenticateUserInDto, AuthTokenOutDto>
{
    public async Task<Result<AuthTokenOutDto>> Execute(AuthenticateUserInDto input)
    {
        var user = await users.FindByEmailOrCpfAsync(input.Login.Trim());
        if (user is null)
            return Result<AuthTokenOutDto>.Failure(AuthErrors.InvalidCredentials);

        if (user.Status == Domain.Enums.ActiveStatus.Inactive)
            return Result<AuthTokenOutDto>.Failure(AuthErrors.InactiveAccount);

        if (!AuthorizationPolicy.CanLogin(user))
            return Result<AuthTokenOutDto>.Failure(AuthErrors.MissingPermissions);

        var hash = user.PasswordHash.Value;
        if (!passwordHasher.IsBcryptHash(hash))
            return Result<AuthTokenOutDto>.Failure(AuthErrors.LegacyPasswordResetRequired);

        if (!passwordHasher.Verify(input.Password, hash))
            return Result<AuthTokenOutDto>.Failure(AuthErrors.InvalidCredentials);

        var (token, expires) = jwt.CreateToken(user);
        var keys = user.Grants.Select(g => g.PermissionKey.Value).ToList();

        return Result<AuthTokenOutDto>.Success(new AuthTokenOutDto(
            token,
            expires,
            user.Id,
            user.TenantId.Value,
            user.Level.ToString(),
            keys));
    }
}
