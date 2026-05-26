using Moq;
using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Application.UseCases;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.Services;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Crm;

public class FindOrCreateByCpfUseCaseTests
{
    private const string ValidCpf = "52998224725";

    [Fact]
    public async Task Execute_WhenCustomerExists_ReturnsExistingWithoutCreating()
    {
        var existing = Customer.Create(
            TenantId.Create(1).Value,
            PersonName.Create("Existing").Value,
            Cpf.Create(ValidCpf).Value).Value;

        var repo = new Mock<ICustomerRepository>();
        repo.Setup(r => r.GetByCpf(It.IsAny<TenantId>(), It.IsAny<Cpf>()))
            .ReturnsAsync(Result<Customer?>.Success(existing));

        var policy = new CustomerRegistrationPolicy(repo.Object);
        var sut = new FindOrCreateByCpfUseCase(repo.Object, policy);

        var result = await sut.Execute((1, new FindOrCreateByCpfInputDto
        {
            Cpf = ValidCpf,
            Name = "Existing",
        }));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.Created);
        Assert.Equal(existing.Id, result.Value.Customer.Id);
        Assert.NotNull(sut.LastFoundEvent);
        repo.Verify(r => r.Save(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenCustomerDoesNotExist_CreatesCustomer()
    {
        var repo = new Mock<ICustomerRepository>();
        repo.Setup(r => r.GetByCpf(It.IsAny<TenantId>(), It.IsAny<Cpf>()))
            .ReturnsAsync(Result<Customer?>.Success(null));
        repo.Setup(r => r.CpfExists(It.IsAny<TenantId>(), It.IsAny<Cpf>(), null))
            .ReturnsAsync(Result<bool>.Success(false));
        repo.Setup(r => r.Save(It.IsAny<Customer>()))
            .ReturnsAsync(Result.Success());

        var policy = new CustomerRegistrationPolicy(repo.Object);
        var sut = new FindOrCreateByCpfUseCase(repo.Object, policy);

        var result = await sut.Execute((1, new FindOrCreateByCpfInputDto
        {
            Cpf = ValidCpf,
            Name = "Novo Cliente",
        }));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Created);
        Assert.NotNull(sut.LastRegisteredEvent);
        repo.Verify(r => r.Save(It.IsAny<Customer>()), Times.Once);
    }
}
