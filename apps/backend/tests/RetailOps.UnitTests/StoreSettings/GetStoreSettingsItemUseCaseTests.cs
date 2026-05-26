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

public class GetStoreSettingsItemUseCaseTests
{
    [Fact]
    public async Task GetPaymentMethod_WhenNotFound_ReturnsFailure()
    {
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(Result<PaymentMethod>.Failure("Payment method not found."));

        var sut = new GetPaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, Guid.NewGuid()));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetCashRegisterTerminal_FromOtherTenant_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(tenantId: 2);
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));

        var sut = new GetCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetPaymentMethod_ReturnsDtoForTenant()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod();
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(method.Id))
            .ReturnsAsync(Result<PaymentMethod>.Success(method));

        var sut = new GetPaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, method.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("Pix", result.Value.Name);
    }

    [Fact]
    public async Task GetPaymentMethod_FromOtherTenant_ReturnsFailure()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod(tenantId: 2);
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetById(method.Id))
            .ReturnsAsync(Result<PaymentMethod>.Success(method));

        var sut = new GetPaymentMethodUseCase(repo.Object);
        var result = await sut.Execute((1, method.Id));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetCashRegisterTerminal_ReturnsDtoForTenant()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));

        var sut = new GetCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("Caixa 1", result.Value.Name);
    }

    [Fact]
    public async Task ListCashRegisterTerminals_ReturnsTenantItems()
    {
        var terminals = new[] { StoreSettingsTestHelpers.CreateCashRegister() };
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<IEnumerable<CashRegisterTerminal>>.Success(terminals));

        var sut = new ListCashRegisterTerminalsUseCase(repo.Object);
        var result = await sut.Execute(1);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }
}

public class StoreSettingsDtoTests
{
    [Fact]
    public void PaymentMethodListItemDto_FromDomain_MapsFields()
    {
        var method = StoreSettingsTestHelpers.CreatePaymentMethod();
        var dto = PaymentMethodListItemDto.FromDomain(method);

        Assert.Equal(method.Id, dto.Id);
        Assert.Equal("Pix", dto.Name);
        Assert.Equal(2.5m, dto.SurchargePercent);
    }

    [Fact]
    public void CashRegisterTerminalListItemDto_FromDomain_MapsFields()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var dto = CashRegisterTerminalListItemDto.FromDomain(terminal);

        Assert.Equal(terminal.Id, dto.Id);
        Assert.Equal("Caixa 1", dto.Name);
        Assert.Equal("Closed", dto.Status);
    }

    [Fact]
    public void CashRegisterTerminalOutputDto_FromDomain_MapsFields()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var dto = CashRegisterTerminalOutputDto.FromDomain(terminal);

        Assert.Equal(terminal.Id, dto.Id);
        Assert.Equal("Caixa 1", dto.Name);
    }
}

public class DiscountTypeExtensionsTests
{
    [Theory]
    [InlineData(DiscountType.None, "N")]
    [InlineData(DiscountType.Percentage, "P")]
    [InlineData(DiscountType.FixedAmount, "F")]
    public void ToLegacyValue_MapsCorrectly(DiscountType type, string legacy)
    {
        Assert.Equal(legacy, type.ToLegacyValue());
    }

    [Theory]
    [InlineData("P", DiscountType.Percentage)]
    [InlineData("F", DiscountType.FixedAmount)]
    [InlineData("X", DiscountType.None)]
    public void FromLegacyValue_MapsCorrectly(string legacy, DiscountType expected)
    {
        Assert.Equal(expected, DiscountTypeExtensions.FromLegacyValue(legacy));
    }
}

public class TerminalStatusExtensionsTests
{
    [Fact]
    public void ToLegacyValue_MapsOpenAndClosed()
    {
        Assert.Equal("Aberto", TerminalStatus.Open.ToLegacyValue());
        Assert.Equal("Fechado", TerminalStatus.Closed.ToLegacyValue());
        Assert.Equal("Fechado", TerminalStatus.Maintenance.ToLegacyValue());
    }

    [Fact]
    public void FromLegacyValue_MapsLegacyStrings()
    {
        Assert.Equal(TerminalStatus.Open, TerminalStatusExtensions.FromLegacyValue("Aberto"));
        Assert.Equal(TerminalStatus.Closed, TerminalStatusExtensions.FromLegacyValue("Fechado"));
        Assert.Equal(TerminalStatus.Closed, TerminalStatusExtensions.FromLegacyValue(null));
    }
}
