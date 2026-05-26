using Moq;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.Services;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Crm;

public class CustomerRegistrationPolicyTests
{
    private const string ValidCpf = "52998224725";

    [Fact]
    public async Task ValidateUniqueCpfAsync_WhenCpfExists_ReturnsFailure()
    {
        var repo = new Mock<ICustomerRepository>();
        repo.Setup(r => r.CpfExists(It.IsAny<TenantId>(), It.IsAny<Cpf>(), null))
            .ReturnsAsync(Result<bool>.Success(true));

        var sut = new CustomerRegistrationPolicy(repo.Object);
        var cpf = Cpf.Create(ValidCpf).Value;
        var tenant = TenantId.Create(1).Value;

        var result = await sut.ValidateUniqueCpfAsync(tenant, cpf);

        Assert.True(result.IsFailure);
        Assert.Contains("already registered", result.Error);
    }

    [Fact]
    public async Task ValidateUniqueCpfAsync_WhenCpfIsUnique_ReturnsSuccess()
    {
        var repo = new Mock<ICustomerRepository>();
        repo.Setup(r => r.CpfExists(It.IsAny<TenantId>(), It.IsAny<Cpf>(), null))
            .ReturnsAsync(Result<bool>.Success(false));

        var sut = new CustomerRegistrationPolicy(repo.Object);
        var cpf = Cpf.Create(ValidCpf).Value;
        var tenant = TenantId.Create(1).Value;

        var result = await sut.ValidateUniqueCpfAsync(tenant, cpf);

        Assert.True(result.IsSuccess);
    }
}
