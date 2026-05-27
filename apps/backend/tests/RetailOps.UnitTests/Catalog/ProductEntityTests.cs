using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Catalog;

public class ProductEntityTests
{
    [Fact]
    public void Create_WhenSalePriceIsZero_SetsOpenPriceFlag()
    {
        var product = BuildProduct(salePrice: 0m, costPrice: 10m);

        Assert.True(product.IsOpenPrice);
        Assert.Equal(0m, product.SalePrice.Value);
    }

    [Fact]
    public void Create_WhenSalePriceIsPositive_DoesNotSetOpenPrice()
    {
        var product = BuildProduct(salePrice: 25.9m, costPrice: 10m);

        Assert.False(product.IsOpenPrice);
    }

    [Fact]
    public void UpdateDetails_WhenSalePriceBecomesZero_SetsOpenPriceFlag()
    {
        var product = BuildProduct(salePrice: 15m, costPrice: 10m);
        var openPrice = SalePrice.Create(0m).Value;

        var result = product.UpdateDetails(salePrice: openPrice);

        Assert.True(result.IsSuccess);
        Assert.True(product.IsOpenPrice);
    }

    private static Product BuildProduct(decimal salePrice, decimal costPrice)
    {
        var tenant = TenantId.Create(1).Value;
        var categoryId = CategoryId.Create(Guid.NewGuid()).Value;

        var result = Product.Create(
            tenant,
            Barcode.Create("7890001112223").Value,
            ProductName.Create("Produto Teste").Value,
            SalePrice.Create(salePrice).Value,
            CostPrice.Create(costPrice).Value,
            StockQuantity.Create(10).Value,
            StockAlertLevel.Create(2).Value,
            categoryId);

        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
