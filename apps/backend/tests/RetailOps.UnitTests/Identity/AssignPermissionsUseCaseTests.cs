using Moq;
using RetailOps.Identity.Core.Application.Dtos;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Application.UseCases;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Identity;

public class AssignPermissionsUseCaseTests
{
    [Fact]
    public async Task Execute_AssignsAndRevokesPermissions()
    {
        var user = User.Reconstitute(
            Guid.Parse("00000000-0000-0000-0000-000000000010"),
            10,
            TenantId.Create(1).Value,
            "User",
            Email.Create("user@test.com").Value,
            null,
            PasswordHash.CreateBcrypt("hash").Value,
            null,
            UserLevel.Operador,
            ActiveStatus.Active,
            TestUsers.Grants("produtos", "vendas")).Value;

        var users = new Mock<IUserRepository>();
        users.Setup(r => r.FindByIdAsync(user.Id, default)).ReturnsAsync(user);
        users.Setup(r => r.SaveAsync(user, default)).ReturnsAsync(Result.Success());

        var sut = new AssignPermissionsUseCase(users.Object);
        var result = await sut.Execute(new AssignPermissionsInDto(user.Id, ["produtos", "estoque"]));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, user.Grants.Count);
        Assert.Contains(user.Grants, g => g.PermissionKey.Value == "produtos");
        Assert.Contains(user.Grants, g => g.PermissionKey.Value == "estoque");
        users.Verify(r => r.SaveAsync(user, default), Times.Once);
    }
}
