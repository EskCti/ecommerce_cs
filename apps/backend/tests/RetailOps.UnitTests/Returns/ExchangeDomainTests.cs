using Moq;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Core.Returns.Domain.Entities;
using RetailOps.Core.Returns.Domain.Services;
using RetailOps.Core.Returns.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Returns;

public class ExchangeQuantityTests
{
    [Fact]
    public void Create_WithOne_ReturnsSuccess()
    {
        var result = ExchangeQuantity.Create(1);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-1)]
    public void Create_WithInvalidValue_ReturnsFailure(int value)
    {
        var result = ExchangeQuantity.Create(value);
        Assert.True(result.IsFailure);
    }
}

public class ExchangeAggregateTests
{
    private static readonly TenantId Tenant = TenantId.Create(1).Value;
    private static readonly CustomerId Customer = CustomerId.Create(Guid.NewGuid()).Value;

    [Fact]
    public void Register_WithInsufficientStock_ReturnsFailure()
    {
        var result = Exchange.Register(
            Tenant,
            Customer,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            Guid.NewGuid(),
            availableOutboundStock: 0);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Register_WithSameProducts_ReturnsFailure()
    {
        var productId = Guid.NewGuid();
        var result = Exchange.Register(
            Tenant,
            Customer,
            productId,
            productId,
            null,
            null,
            Guid.NewGuid(),
            availableOutboundStock: 1);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Register_WithValidData_ReturnsExchange()
    {
        var result = Exchange.Register(
            Tenant,
            Customer,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            Guid.NewGuid(),
            availableOutboundStock: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Quantity.Value);
    }
}

public class ExchangeStockPolicyTests
{
    [Fact]
    public async Task ApplyRegisterAsync_AdjustsInboundAndOutbound()
    {
        var catalog = new Mock<IProductCatalogService>();
        catalog.Setup(s => s.GetAvailableStockAsync(It.IsAny<TenantId>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(3));
        catalog.Setup(s => s.ReleaseStockAsync(It.IsAny<TenantId>(), It.IsAny<Guid>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        catalog.Setup(s => s.ReserveStockAsync(It.IsAny<TenantId>(), It.IsAny<Guid>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var repo = new Mock<IProductRepository>();
        var sut = new ExchangeStockPolicy(catalog.Object, repo.Object);
        var tenant = TenantId.Create(1).Value;

        var result = await sut.ApplyRegisterAsync(
            tenant,
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            null);

        Assert.True(result.IsSuccess);
        catalog.Verify(s => s.ReleaseStockAsync(tenant, It.IsAny<Guid>(), 1, It.IsAny<CancellationToken>()), Times.Once);
        catalog.Verify(s => s.ReserveStockAsync(tenant, It.IsAny<Guid>(), 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApplyDeleteReverseAsync_ReversesStock()
    {
        var catalog = new Mock<IProductCatalogService>();
        catalog.Setup(s => s.ReleaseStockAsync(It.IsAny<TenantId>(), It.IsAny<Guid>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        catalog.Setup(s => s.ReserveStockAsync(It.IsAny<TenantId>(), It.IsAny<Guid>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var repo = new Mock<IProductRepository>();
        var sut = new ExchangeStockPolicy(catalog.Object, repo.Object);
        var tenant = TenantId.Create(1).Value;

        var result = await sut.ApplyDeleteReverseAsync(
            tenant,
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            null);

        Assert.True(result.IsSuccess);
    }
}
