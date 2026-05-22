using Moq;
using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Application.UseCases;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.Services;
using RetailOps.Identity.Core.Domain.ValueObjects;
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
