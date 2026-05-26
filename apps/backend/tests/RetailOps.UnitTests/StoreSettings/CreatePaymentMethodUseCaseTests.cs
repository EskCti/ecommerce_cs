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

public class CreatePaymentMethodUseCaseTests
{
    [Fact]
    public async Task Execute_WhenNameExists_ReturnsFailure()
    {
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.NameExists(It.IsAny<TenantId>(), It.IsAny<PaymentMethodName>(), null))
            .ReturnsAsync(Result<bool>.Success(true));

        var sut = new CreatePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, new CreatePaymentMethodInputDto
        {
            Name = "Pix",
            SurchargePercent = 0,
        }));

        Assert.True(result.IsFailure);
        Assert.Contains("already exists", result.Error);
    }

    [Fact]
    public async Task Execute_WithValidInput_SavesAndReturnsDto()
    {
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.NameExists(It.IsAny<TenantId>(), It.IsAny<PaymentMethodName>(), null))
            .ReturnsAsync(Result<bool>.Success(false));
        repo.Setup(r => r.Save(It.IsAny<PaymentMethod>()))
            .ReturnsAsync(Result.Success());

        var sut = new CreatePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, new CreatePaymentMethodInputDto
        {
            Name = "Pix",
            SurchargePercent = 2.5m,
            IsActive = true,
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal("Pix", result.Value.Name);
        repo.Verify(r => r.Save(It.IsAny<PaymentMethod>()), Times.Once);
    }

    [Fact]
    public async Task Execute_WithInvalidName_ReturnsFailure()
    {
        var repo = new Mock<IPaymentMethodRepository>();
        var sut = new CreatePaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, new CreatePaymentMethodInputDto
        {
            Name = "A",
            SurchargePercent = 0,
        }));

        Assert.True(result.IsFailure);
    }
}
