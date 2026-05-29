using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Catalog;

public class GradeVariantSyncServiceTests
{
    private readonly GradeVariantSyncService _sync = new();

    [Fact]
    public void RebuildCartesianVariants_WithTwoDimensions_CreatesFourVariants()
    {
        var product = BuildProductWithTwoDimensions();

        var result = _sync.RebuildCartesianVariants(product);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, product.GradeVariants.Count);
        Assert.All(product.GradeVariants, v => Assert.Equal(0, v.Stock.Value));
    }

    [Fact]
    public void MigrateOptionsStockToVariants_SingleDimension_CopiesOptionStock()
    {
        var product = BuildProductWithOneDimension(stock: 12);

        var result = _sync.MigrateOptionsStockToVariants(product);

        Assert.True(result.IsSuccess);
        Assert.Single(product.GradeVariants);
        Assert.Equal(12, product.GradeVariants[0].Stock.Value);
    }

    [Fact]
    public void AddGradeDimension_ThirdDimension_IsRejected()
    {
        var product = BuildProductWithTwoDimensions();
        var third = GradeDimension.Create(product.Id, ProductName.Create("Material").Value).Value;

        var result = product.AddGradeDimension(third);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void GradeVariant_ReferencesOption_WhenCombinationIncludesIt()
    {
        var product = BuildProductWithTwoDimensions();
        _sync.RebuildCartesianVariants(product);

        var optionId = product.GradeDimensions[0].Options[0].Id;
        var variant = product.GradeVariants.First(v => v.ReferencesOption(optionId));

        Assert.True(variant.ReferencesOption(optionId));
    }

    [Fact]
    public void FindVariantByOptions_ReturnsMatchingVariant()
    {
        var product = BuildProductWithTwoDimensions();
        _sync.RebuildCartesianVariants(product);

        var optionIds = product.GradeDimensions[0].Options[0].Id;
        var optionIds2 = product.GradeDimensions[1].Options[1].Id;

        var variant = product.FindVariantByOptions([optionIds, optionIds2]).Value;

        Assert.NotNull(variant);
        Assert.Equal(2, variant!.OptionIds.Count);
    }

    private static Product BuildProductWithOneDimension(int stock = 5)
    {
        var tenant = TenantId.Create(1).Value;
        var categoryId = CategoryId.Create(Guid.NewGuid()).Value;
        var product = Product.Create(
            tenant,
            Barcode.Create("7890001112223").Value,
            ProductName.Create("Camiseta").Value,
            SalePrice.Create(10m).Value,
            CostPrice.Create(5m).Value,
            StockQuantity.Create(0).Value,
            StockAlertLevel.Create(1).Value,
            categoryId).Value;

        var dimension = GradeDimension.Create(product.Id, ProductName.Create("Tamanho").Value).Value;
        dimension.AddOption(ProductName.Create("P").Value, StockQuantity.Create(stock).Value);
        product.AddGradeDimension(dimension);

        return product;
    }

    private static Product BuildProductWithTwoDimensions()
    {
        var tenant = TenantId.Create(1).Value;
        var categoryId = CategoryId.Create(Guid.NewGuid()).Value;
        var product = Product.Create(
            tenant,
            Barcode.Create("7890001112224").Value,
            ProductName.Create("Camiseta 2D").Value,
            SalePrice.Create(10m).Value,
            CostPrice.Create(5m).Value,
            StockQuantity.Create(0).Value,
            StockAlertLevel.Create(1).Value,
            categoryId).Value;

        var cor = GradeDimension.Create(product.Id, ProductName.Create("Cor").Value).Value;
        cor.AddOption(ProductName.Create("Azul").Value, StockQuantity.Create(0).Value);
        cor.AddOption(ProductName.Create("Vermelho").Value, StockQuantity.Create(0).Value);
        product.AddGradeDimension(cor);

        var tamanho = GradeDimension.Create(product.Id, ProductName.Create("Tamanho").Value).Value;
        tamanho.AddOption(ProductName.Create("P").Value, StockQuantity.Create(0).Value);
        tamanho.AddOption(ProductName.Create("M").Value, StockQuantity.Create(0).Value);
        product.AddGradeDimension(tamanho);

        return product;
    }
}
