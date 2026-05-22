using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Domain.Services;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Core.Application.UseCases;

public sealed class AuthenticateUserUseCase(
    IUserRepository users,
    ILegacyMd5PasswordVerifier legacyMd5,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwt) : IUseCase<AuthenticateUserInDto, AuthTokenOutDto>
{
    public async Task<Result<AuthTokenOutDto>> Execute(AuthenticateUserInDto input)
    {
        var user = await users.FindByEmailOrCpfAsync(input.Login.Trim());
        if (user is null)
            return Result<AuthTokenOutDto>.Failure("Invalid credentials.");

        if (!AuthorizationPolicy.CanLogin(user))
            return Result<AuthTokenOutDto>.Failure("User has no permissions assigned.");

        var hash = user.PasswordHash.Value;
        var valid = passwordHasher.IsBcryptHash(hash)
            ? passwordHasher.Verify(input.Password, hash)
            : legacyMd5.Verify(input.Password, hash);

        if (!valid)
            return Result<AuthTokenOutDto>.Failure("Invalid credentials.");

        if (!passwordHasher.IsBcryptHash(hash))
        {
            user.UpdatePasswordHash(
                Domain.ValueObjects.PasswordHash.CreateBcrypt(passwordHasher.Hash(input.Password)).Value);
            await users.SaveAsync(user);
        }

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
