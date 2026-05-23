using Moq;
using RetailOps.Identity.Core.Application;
using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Application.UseCases;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.Services;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Identity;

public class AuthenticateUserUseCaseTests
{
    [Fact]
    public async Task Execute_ReturnsFailure_WhenUserNotFound()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(r => r.FindByEmailOrCpfAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);

        var sut = new AuthenticateUserUseCase(
            users.Object,
            Mock.Of<ILegacyMd5PasswordVerifier>(),
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtTokenService>());

        var result = await sut.Execute(new AuthenticateUserInDto("a@b.com", "secret"));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Execute_Returns403Message_WhenUserInactive()
    {
        var user = User.Reconstitute(
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            2,
            TenantId.Create(1).Value,
            "Inactive",
            Email.Create("inactive@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            null,
            UserLevel.Gerente,
            ActiveStatus.Inactive,
            TestUsers.Grants("produtos")).Value;

        var users = new Mock<IUserRepository>();
        users.Setup(r => r.FindByEmailOrCpfAsync("inactive@test.com", default)).ReturnsAsync(user);

        var sut = new AuthenticateUserUseCase(
            users.Object,
            Mock.Of<ILegacyMd5PasswordVerifier>(),
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtTokenService>());

        var result = await sut.Execute(new AuthenticateUserInDto("inactive@test.com", "secret"));
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.InactiveAccount, result.Error);
    }

    [Fact]
    public async Task Execute_Returns403Message_WhenOperatorHasNoGrants()
    {
        var user = User.Reconstitute(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            3,
            TenantId.Create(1).Value,
            "Operator",
            Email.Create("op@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            null,
            UserLevel.Operador,
            ActiveStatus.Active,
            []).Value;

        var users = new Mock<IUserRepository>();
        users.Setup(r => r.FindByEmailOrCpfAsync("op@test.com", default)).ReturnsAsync(user);

        var sut = new AuthenticateUserUseCase(
            users.Object,
            Mock.Of<ILegacyMd5PasswordVerifier>(),
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtTokenService>());

        var result = await sut.Execute(new AuthenticateUserInDto("op@test.com", "secret"));
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrors.MissingPermissions, result.Error);
    }

    [Fact]
    public async Task Execute_RehashesLegacyMd5_AndSavesUser()
    {
        const string md5Hash = "5ebe2294ecd0e0f08eab7690d2a6ee69";
        var user = User.Reconstitute(
            Guid.Parse("00000000-0000-0000-0000-000000000004"),
            4,
            TenantId.Create(1).Value,
            "Legacy",
            Email.Create("legacy@test.com").Value,
            null,
            PasswordHash.CreateBcrypt(md5Hash).Value,
            null,
            UserLevel.Administrador,
            ActiveStatus.Active,
            []).Value;

        var users = new Mock<IUserRepository>();
        users.Setup(r => r.FindByEmailOrCpfAsync("legacy@test.com", default)).ReturnsAsync(user);
        users.Setup(r => r.SaveAsync(user, default)).ReturnsAsync(Result.Success());

        var legacy = new Mock<ILegacyMd5PasswordVerifier>();
        legacy.Setup(l => l.Verify("secret", md5Hash)).Returns(true);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.IsBcryptHash(md5Hash)).Returns(false);
        hasher.Setup(h => h.Hash("secret")).Returns("$2a$11$newbcrypt");

        var jwt = new Mock<IJwtTokenService>();
        jwt.Setup(j => j.CreateToken(user)).Returns(("token", DateTime.UtcNow.AddHours(1)));

        var sut = new AuthenticateUserUseCase(users.Object, legacy.Object, hasher.Object, jwt.Object);
        var result = await sut.Execute(new AuthenticateUserInDto("legacy@test.com", "secret"));

        Assert.True(result.IsSuccess);
        users.Verify(r => r.SaveAsync(user, default), Times.Once);
        hasher.Verify(h => h.Hash("secret"), Times.Once);
    }

    [Fact]
    public async Task Execute_ReturnsToken_WhenCredentialsValid()
    {
        var user = User.Reconstitute(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            1,
            TenantId.Create(1).Value,
            "Admin",
            Email.Create("admin@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("$2a$11$abcdefghijklmnopqrstuv").Value,
            null,
            UserLevel.Administrador,
            ActiveStatus.Active,
            []).Value;

        var users = new Mock<IUserRepository>();
        users.Setup(r => r.FindByEmailOrCpfAsync("admin@test.com", default)).ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.IsBcryptHash(It.IsAny<string>())).Returns(true);
        hasher.Setup(h => h.Verify("secret", It.IsAny<string>())).Returns(true);

        var jwt = new Mock<IJwtTokenService>();
        jwt.Setup(j => j.CreateToken(user)).Returns(("token", DateTime.UtcNow.AddHours(1)));

        var sut = new AuthenticateUserUseCase(users.Object, Mock.Of<ILegacyMd5PasswordVerifier>(), hasher.Object, jwt.Object);
        var result = await sut.Execute(new AuthenticateUserInDto("admin@test.com", "secret"));

        Assert.True(result.IsSuccess);
        Assert.Equal("token", result.Value.AccessToken);
    }
}
