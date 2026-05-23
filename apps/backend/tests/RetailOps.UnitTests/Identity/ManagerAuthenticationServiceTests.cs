using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.Services;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Identity;

public class ManagerAuthenticationServiceTests
{
    private readonly ManagerAuthenticationService _sut = new();

    [Fact]
    public void Verify_Succeeds_WhenPinMatchesBcryptHash()
    {
        const string pin = "1234";
        var bcrypt = BCrypt.Net.BCrypt.HashPassword(pin);
        var user = CreateManager(bcrypt);

        var result = _sut.Verify(user, pin, (plain, hash) => BCrypt.Net.BCrypt.Verify(plain, hash));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Verify_Fails_WhenPinIsWrong()
    {
        var bcrypt = BCrypt.Net.BCrypt.HashPassword("1234");
        var user = CreateManager(bcrypt);

        var result = _sut.Verify(user, "9999", (plain, hash) => BCrypt.Net.BCrypt.Verify(plain, hash));

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid manager PIN.", result.Error);
    }

    [Fact]
    public void Verify_Fails_WhenPinNotConfigured()
    {
        var user = CreateManager(null);

        var result = _sut.Verify(user, "1234", (plain, hash) => BCrypt.Net.BCrypt.Verify(plain, hash));

        Assert.True(result.IsFailure);
        Assert.Equal("Manager PIN not configured.", result.Error);
    }

    [Fact]
    public void Verify_Fails_WhenUserLevelIsOperator()
    {
        var bcrypt = BCrypt.Net.BCrypt.HashPassword("1234");
        var user = User.Reconstitute(
            Guid.NewGuid(),
            1,
            TenantId.Create(1).Value,
            "Op",
            null,
            null,
            PasswordHash.CreateBcrypt("hash").Value,
            ManagerPin.FromBcrypt(bcrypt).Value,
            UserLevel.Operador,
            ActiveStatus.Active,
            []).Value;

        var result = _sut.Verify(user, "1234", (plain, hash) => BCrypt.Net.BCrypt.Verify(plain, hash));

        Assert.True(result.IsFailure);
        Assert.Equal("User level cannot verify manager PIN.", result.Error);
    }

    private static User CreateManager(string? bcryptPin)
    {
        ManagerPin? pin = bcryptPin is null ? null : ManagerPin.FromBcrypt(bcryptPin).Value;
        return User.Reconstitute(
            Guid.NewGuid(),
            1,
            TenantId.Create(1).Value,
            "Manager",
            null,
            null,
            PasswordHash.CreateBcrypt("hash").Value,
            pin,
            UserLevel.Gerente,
            ActiveStatus.Active,
            []).Value;
    }
}
