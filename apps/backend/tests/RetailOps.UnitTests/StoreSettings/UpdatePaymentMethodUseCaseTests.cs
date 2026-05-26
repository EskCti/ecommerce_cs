using Moq;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Application.UseCases;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class UpdatePaymentMethodUseCaseTests
{
    [Fact]
    public async Task Execute_WithValidInput_UpdatesPaymentMethod()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod();
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(method.Id))
            .ReturnsAsync(Result<PaymentMethod>.Success(method));
        repo.Setup(r => r.NameExists(It.IsAny<TenantId>(), It.IsAny<PaymentMethodName>(), method.Id))
            .ReturnsAsync(Result<bool>.Success(false));
        repo.Setup(r => r.Save(It.IsAny<PaymentMethod>()))
            .ReturnsAsync(Result.Success());

        var sut = new UpdatePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, method.Id, new UpdatePaymentMethodInputDto
        {
            Name = "Cartão",
            SurchargePercent = 4,
            IsActive = false,
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal("Cartão", result.Value.Name);
        Assert.False(result.Value.IsActive);
    }

    [Fact]
    public async Task Execute_FromOtherTenant_ReturnsFailure()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod(tenantId: 2);
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(method.Id))
            .ReturnsAsync(Result<PaymentMethod>.Success(method));

        var sut = new UpdatePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, method.Id, new UpdatePaymentMethodInputDto { Name = "Hack" }));

        Assert.True(result.IsFailure);
        Assert.Contains("does not belong", result.Error);
    }

    [Fact]
    public async Task Execute_WithDuplicateName_ReturnsFailure()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod();
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(method.Id))
            .ReturnsAsync(Result<PaymentMethod>.Success(method));
        repo.Setup(r => r.NameExists(It.IsAny<TenantId>(), It.IsAny<PaymentMethodName>(), method.Id))
            .ReturnsAsync(Result<bool>.Success(true));

        var sut = new UpdatePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, method.Id, new UpdatePaymentMethodInputDto { Name = "Duplicado" }));

        Assert.True(result.IsFailure);
        Assert.Contains("already exists", result.Error);
    }

    [Fact]
    public async Task Execute_WithInvalidSurcharge_ReturnsFailure()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod();
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(method.Id))
            .ReturnsAsync(Result<PaymentMethod>.Success(method));

        var sut = new UpdatePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, method.Id, new UpdatePaymentMethodInputDto
        {
            SurchargePercent = -1,
        }));

        Assert.True(result.IsFailure);
    }
}
