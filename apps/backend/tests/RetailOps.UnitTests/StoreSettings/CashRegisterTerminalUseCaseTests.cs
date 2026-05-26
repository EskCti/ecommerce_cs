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

public class CashRegisterTerminalUseCaseTests
{
    [Fact]
    public async Task Create_WithValidInput_SavesTerminal()
    {
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<TenantId>(), It.IsAny<CashRegisterTerminalName>()))
            .ReturnsAsync(Result<CashRegisterTerminal>.Failure("Cash register terminal not found."));
        repo.Setup(r => r.Save(It.IsAny<CashRegisterTerminal>()))
            .ReturnsAsync(Result.Success());

        var sut = new CreateCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, new CreateCashRegisterTerminalInputDto
        {
            Name = "Caixa 1",
            Status = "Closed",
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal("Caixa 1", result.Value.Name);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsFailure()
    {
        var existing = StoreSettingsTestHelpers.CreateCashRegister();
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<TenantId>(), It.IsAny<CashRegisterTerminalName>()))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(existing));

        var sut = new CreateCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, new CreateCashRegisterTerminalInputDto { Name = "Caixa 1" }));

        Assert.True(result.IsFailure);
        Assert.Contains("already exists", result.Error);
    }

    [Fact]
    public async Task Update_ChangesNameAndStatus()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));
        repo.Setup(r => r.GetByName(It.IsAny<TenantId>(), It.IsAny<CashRegisterTerminalName>()))
            .ReturnsAsync(Result<CashRegisterTerminal>.Failure("Cash register terminal not found."));
        repo.Setup(r => r.Save(It.IsAny<CashRegisterTerminal>()))
            .ReturnsAsync(Result.Success());

        var operatorId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var sut = new UpdateCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id, new UpdateCashRegisterTerminalInputDto
        {
            Name = "Caixa Principal",
            Status = "Open",
            AssignedOperatorId = operatorId,
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal("Caixa Principal", result.Value.Name);
        Assert.Equal("Open", result.Value.Status);
    }

    [Fact]
    public async Task Delete_WhenClosed_RemovesTerminal()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));
        repo.Setup(r => r.Delete(terminal.Id))
            .ReturnsAsync(Result.Success());

        var sut = new DeleteCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task Delete_WhenOpen_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(status: TerminalStatus.Open);
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));

        var sut = new DeleteCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id));

        Assert.True(result.IsFailure);
        Assert.Contains("open", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Update_WithInvalidStatus_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));

        var sut = new UpdateCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id, new UpdateCashRegisterTerminalInputDto
        {
            Status = "Invalid",
        }));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Update_FromOtherTenant_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(tenantId: 2);
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));

        var sut = new UpdateCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id, new UpdateCashRegisterTerminalInputDto { Name = "Hack" }));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Create_WithInvalidStatus_ReturnsFailure()
    {
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<TenantId>(), It.IsAny<CashRegisterTerminalName>()))
            .ReturnsAsync(Result<CashRegisterTerminal>.Failure("Cash register terminal not found."));

        var sut = new CreateCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, new CreateCashRegisterTerminalInputDto
        {
            Name = "Caixa",
            Status = "Invalid",
        }));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Update_PutsTerminalInMaintenance()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));
        repo.Setup(r => r.Save(It.IsAny<CashRegisterTerminal>()))
            .ReturnsAsync(Result.Success());

        var sut = new UpdateCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id, new UpdateCashRegisterTerminalInputDto
        {
            Status = "Maintenance",
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal("Maintenance", result.Value.Status);
    }

    [Fact]
    public async Task Update_WithDuplicateName_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var other = StoreSettingsTestHelpers.CreateCashRegister(legacyId: 2, name: "Caixa 2");
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));
        repo.Setup(r => r.GetByName(It.IsAny<TenantId>(), It.IsAny<CashRegisterTerminalName>()))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(other));

        var sut = new UpdateCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id, new UpdateCashRegisterTerminalInputDto { Name = "Caixa 2" }));

        Assert.True(result.IsFailure);
        Assert.Contains("already exists", result.Error);
    }

    [Fact]
    public async Task Update_PutsTerminalOutOfService()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(terminal.Id))
            .ReturnsAsync(Result<CashRegisterTerminal>.Success(terminal));
        repo.Setup(r => r.Save(It.IsAny<CashRegisterTerminal>()))
            .ReturnsAsync(Result.Success());

        var sut = new UpdateCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, terminal.Id, new UpdateCashRegisterTerminalInputDto
        {
            Status = "OutOfService",
        }));

        Assert.True(result.IsSuccess);
        Assert.Equal("OutOfService", result.Value.Status);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsFailure()
    {
        var repo = new Mock<ICashRegisterTerminalRepository>();
        repo.Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(Result<CashRegisterTerminal>.Failure("Cash register terminal not found."));

        var sut = new DeleteCashRegisterTerminalUseCase(repo.Object);
        var result = await sut.Execute((1, Guid.NewGuid()));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task List_ReturnsTenantPaymentMethods()
    {
        var methods = new[] { StoreSettingsTestHelpers.CreatePaymentMethod() };
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetByTenantId(It.IsAny<TenantId>()))
            .ReturnsAsync(Result<IEnumerable<PaymentMethod>>.Success(methods));

        var sut = new ListPaymentMethodsUseCase(repo.Object);
        var result = await sut.Execute(1);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }
}
