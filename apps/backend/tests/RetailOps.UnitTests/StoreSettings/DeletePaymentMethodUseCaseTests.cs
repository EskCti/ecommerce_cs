using Moq;
using RetailOps.Core.StoreSettings.Application.UseCases;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class DeletePaymentMethodUseCaseTests
{
    [Fact]
    public async Task Execute_WhenNotFound_ReturnsFailure()
    {
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(Result<PaymentMethod>.Failure("Payment method not found."));

        var sut = new DeletePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, Guid.NewGuid()));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Execute_WhenDifferentTenant_ReturnsFailure()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod(tenantId: 2);
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(method.Id))
            .ReturnsAsync(Result<PaymentMethod>.Success(method));

        var sut = new DeletePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, method.Id));

        Assert.True(result.IsFailure);
        Assert.Contains("does not belong", result.Error);
    }

    [Fact]
    public async Task Execute_WhenSameTenant_Deletes()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod(tenantId: 1);
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(method.Id))
            .ReturnsAsync(Result<PaymentMethod>.Success(method));
        repo.Setup(r => r.Delete(method.Id))
            .ReturnsAsync(Result.Success());

        var sut = new DeletePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, method.Id));

        Assert.True(result.IsSuccess);
        repo.Verify(r => r.Delete(method.Id), Times.Once);
    }
}
