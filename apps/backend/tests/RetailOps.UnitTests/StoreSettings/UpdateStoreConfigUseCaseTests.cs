using Moq;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Application.UseCases;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class UpdateStoreConfigUseCaseTests
{
    [Fact]
    public async Task Execute_UpdatesExistingConfig()
    {
        var existing = StoreSettingsTestHelpers.CreateStoreConfig();
        var repo = new Mock<IStoreConfigRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Success(existing));
        repo.Setup(r => r.Save(It.IsAny<StoreConfig>()))
            .ReturnsAsync(Result.Success());

        var sut = new UpdateStoreConfigUseCase(repo.Object);
        var result = await sut.Execute((1, new UpdateStoreConfigInputDto
        {
            Name = "Loja Nova",
            DiscountType = "FixedAmount",
            DiscountValue = 15,
            CommissionRate = 3,
            ReportFormat = "HTML",
            Contacts = "novo@test.com",
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal("Loja Nova", result.Value.Name);
        Assert.Equal("HTML", result.Value.ReportFormat);
    }

    [Fact]
    public async Task Execute_WhenMissing_CreatesThenUpdates()
    {
        var created = StoreSettingsTestHelpers.CreateStoreConfig();
        var repo = new Mock<IStoreConfigRepository>();
        repo.SetupSequence(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Failure("Store config not found."))
            .ReturnsAsync(Result<StoreConfig>.Success(created));
        repo.Setup(r => r.Save(It.IsAny<StoreConfig>()))
            .ReturnsAsync(Result.Success());

        var sut = new UpdateStoreConfigUseCase(repo.Object);
        var result = await sut.Execute((1, new UpdateStoreConfigInputDto { Name = "Loja Criada" }));

        Assert.True(result.IsSuccess);
        Assert.Equal("Loja Criada", result.Value.Name);
    }

    [Fact]
    public async Task Execute_WithInvalidDiscountType_ReturnsFailure()
    {
        var repo = new Mock<IStoreConfigRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Success(StoreSettingsTestHelpers.CreateStoreConfig()));

        var sut = new UpdateStoreConfigUseCase(repo.Object);
        var result = await sut.Execute((1, new UpdateStoreConfigInputDto { DiscountType = "Invalid" }));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Execute_SaveFailure_ReturnsFailure()
    {
        var repo = new Mock<IStoreConfigRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Success(StoreSettingsTestHelpers.CreateStoreConfig()));
        repo.Setup(r => r.Save(It.IsAny<StoreConfig>()))
            .ReturnsAsync(Result.Failure("save failed"));

        var sut = new UpdateStoreConfigUseCase(repo.Object);
        var result = await sut.Execute((1, new UpdateStoreConfigInputDto { Name = "Falha" }));

        Assert.True(result.IsFailure);
        Assert.Equal("save failed", result.Error);
    }

    [Fact]
    public async Task Execute_UpdatesCnpjAndLogoPath()
    {
        var repo = new Mock<IStoreConfigRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Success(StoreSettingsTestHelpers.CreateStoreConfig()));
        repo.Setup(r => r.Save(It.IsAny<StoreConfig>()))
            .ReturnsAsync(Result.Success());

        var sut = new UpdateStoreConfigUseCase(repo.Object);
        var result = await sut.Execute((1, new UpdateStoreConfigInputDto
        {
            Cnpj = "12.345.678/0001-95",
            LogoPath = "/images/logo.png",
            ApiToken = "1234567890",
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal("12345678000195", result.Value.Cnpj);
        Assert.Equal("/images/logo.png", result.Value.LogoPath);
    }

    [Fact]
    public async Task Execute_WithInvalidCommissionRate_ReturnsFailure()
    {
        var repo = new Mock<IStoreConfigRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Success(StoreSettingsTestHelpers.CreateStoreConfig()));

        var sut = new UpdateStoreConfigUseCase(repo.Object);
        var result = await sut.Execute((1, new UpdateStoreConfigInputDto { CommissionRate = 101 }));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Execute_WhenCreateDefaultSaveFails_ReturnsFailure()
    {
        var repo = new Mock<IStoreConfigRepository>();
        repo.SetupSequence(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Failure("Store config not found."))
            .ReturnsAsync(Result<StoreConfig>.Success(StoreSettingsTestHelpers.CreateStoreConfig()));
        repo.Setup(r => r.Save(It.IsAny<StoreConfig>()))
            .ReturnsAsync(Result.Failure("cannot create"));

        var sut = new UpdateStoreConfigUseCase(repo.Object);
        var result = await sut.Execute((1, new UpdateStoreConfigInputDto { Name = "Nova" }));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Execute_WithInvalidStoreName_ReturnsFailure()
    {
        var repo = new Mock<IStoreConfigRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Success(StoreSettingsTestHelpers.CreateStoreConfig()));

        var sut = new UpdateStoreConfigUseCase(repo.Object);
        var result = await sut.Execute((1, new UpdateStoreConfigInputDto { Name = "  " }));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Execute_WithInvalidApiToken_ReturnsFailure()
    {
        var repo = new Mock<IStoreConfigRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Success(StoreSettingsTestHelpers.CreateStoreConfig()));

        var sut = new UpdateStoreConfigUseCase(repo.Object);
        var result = await sut.Execute((1, new UpdateStoreConfigInputDto { ApiToken = "short" }));

        Assert.True(result.IsFailure);
    }
}
