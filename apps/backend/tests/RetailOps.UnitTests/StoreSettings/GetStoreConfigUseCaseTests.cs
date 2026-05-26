using Moq;
using RetailOps.Core.StoreSettings.Application.UseCases;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class GetStoreConfigUseCaseTests
{
    [Fact]
    public async Task Execute_WhenMissing_CreatesDefaultConfig()
    {
        var repo = new Mock<IStoreConfigRepository>();
        repo.SetupSequence(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Failure("Store config not found"))
            .ReturnsAsync(Result<StoreConfig>.Success(StoreSettingsTestHelpers.CreateStoreConfig()));
        repo.Setup(r => r.Save(It.IsAny<StoreConfig>()))
            .ReturnsAsync(Result.Success());

        var sut = new GetStoreConfigUseCase(repo.Object);
        var result = await sut.Execute(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Minha Loja", result.Value.Name);
        repo.Verify(r => r.Save(It.IsAny<StoreConfig>()), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenExists_ReturnsExistingConfig()
    {
        var existing = StoreSettingsTestHelpers.CreateStoreConfig();
        var repo = new Mock<IStoreConfigRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Success(existing));

        var sut = new GetStoreConfigUseCase(repo.Object);
        var result = await sut.Execute(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Loja Teste", result.Value.Name);
    }

    [Fact]
    public async Task Execute_WhenDefaultSaveFails_ReturnsFailure()
    {
        var repo = new Mock<IStoreConfigRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<StoreConfig>.Failure("Store config not found"));
        repo.Setup(r => r.Save(It.IsAny<StoreConfig>()))
            .ReturnsAsync(Result.Failure("save failed"));

        var sut = new GetStoreConfigUseCase(repo.Object);
        var result = await sut.Execute(1);

        Assert.True(result.IsFailure);
    }
}
