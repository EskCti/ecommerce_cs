using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.Services;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Identity;

public class AuthorizationPolicyTests
{
    [Theory]
    [InlineData(UserLevel.Sas, true)]
    [InlineData(UserLevel.Administrador, true)]
    [InlineData(UserLevel.Gerente, false)]
    public void IsPrivileged_MatchesLevel(UserLevel level, bool expected) =>
        Assert.Equal(expected, AuthorizationPolicy.IsPrivileged(level));

    [Fact]
    public void CanLogin_BlocksInactiveUser()
    {
        var user = CreateUser(UserLevel.Gerente, ActiveStatus.Inactive, ["produtos"]);
        Assert.False(AuthorizationPolicy.CanLogin(user));
    }

    [Fact]
    public void CanLogin_AllowsPrivilegedWithoutGrants()
    {
        var user = CreateUser(UserLevel.Administrador, ActiveStatus.Active, []);
        Assert.True(AuthorizationPolicy.CanLogin(user));
    }

    [Fact]
    public void CanLogin_BlocksOperatorWithoutGrants()
    {
        var user = CreateUser(UserLevel.Operador, ActiveStatus.Active, []);
        Assert.False(AuthorizationPolicy.CanLogin(user));
    }

    private static User CreateUser(UserLevel level, ActiveStatus status, string[] keys)
    {
        var grants = keys.Select(k => PermissionGrant.Create(Guid.NewGuid(), PermissionKey.Create(k).Value)).ToList();
        return User.Reconstitute(
            Guid.NewGuid(),
            1,
            TenantId.Create(1).Value,
            "Test",
            null,
            null,
            PasswordHash.CreateBcrypt("hash").Value,
            null,
            level,
            status,
            grants).Value;
    }
}
