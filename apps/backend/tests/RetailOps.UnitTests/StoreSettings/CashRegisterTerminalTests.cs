using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.StoreSettings;

public class CashRegisterTerminalTests
{
    [Fact]
    public void Create_DefaultsToClosed()
    {
        var result = CashRegisterTerminal.Create(
            StoreSettingsTestHelpers.Tenant(),
            CashRegisterTerminalName.Create("Caixa 1").Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(TerminalStatus.Closed, result.Value.Status);
    }

    [Fact]
    public void Open_FromClosed_Succeeds()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var operatorId = UserId.Create(Guid.Parse("00000000-0000-0000-0000-000000000002")).Value;
        var result = terminal.Open(operatorId);

        Assert.True(result.IsSuccess);
        Assert.Equal(TerminalStatus.Open, terminal.Status);
        Assert.Equal(operatorId, terminal.AssignedOperatorId);
    }

    [Fact]
    public void Open_WhenAlreadyOpen_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(status: TerminalStatus.Open);
        var result = terminal.Open();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Close_FromOpen_Succeeds()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(status: TerminalStatus.Open);
        var result = terminal.Close();

        Assert.True(result.IsSuccess);
        Assert.Equal(TerminalStatus.Closed, terminal.Status);
        Assert.Null(terminal.AssignedOperatorId);
    }

    [Fact]
    public void PutInMaintenance_WhenOpen_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(status: TerminalStatus.Open);
        var result = terminal.PutInMaintenance();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void PutInMaintenance_WhenClosed_Succeeds()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var result = terminal.PutInMaintenance();

        Assert.True(result.IsSuccess);
        Assert.Equal(TerminalStatus.Maintenance, terminal.Status);
    }

    [Fact]
    public void TakeOutOfService_WhenClosed_Succeeds()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var result = terminal.TakeOutOfService();

        Assert.True(result.IsSuccess);
        Assert.Equal(TerminalStatus.OutOfService, terminal.Status);
    }

    [Fact]
    public void Open_WhenOutOfService_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(status: TerminalStatus.OutOfService);
        var result = terminal.Open();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var result = terminal.Close();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void AssignOperator_WhenOpen_Succeeds()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(status: TerminalStatus.Open);
        var operatorId = UserId.Create(Guid.Parse("00000000-0000-0000-0000-000000000002")).Value;
        var result = terminal.AssignOperator(operatorId);

        Assert.True(result.IsSuccess);
        Assert.Equal(operatorId, terminal.AssignedOperatorId);
    }

    [Fact]
    public void AssignOperator_WhenClosed_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var operatorId = UserId.Create(Guid.Parse("00000000-0000-0000-0000-000000000002")).Value;
        var result = terminal.AssignOperator(operatorId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void UnassignOperator_WhenOpen_Succeeds()
    {
        var operatorId = UserId.Create(Guid.Parse("00000000-0000-0000-0000-000000000002")).Value;
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(status: TerminalStatus.Open);
        terminal.AssignOperator(operatorId);

        var result = terminal.UnassignOperator();

        Assert.True(result.IsSuccess);
        Assert.Null(terminal.AssignedOperatorId);
    }

    [Fact]
    public void TakeOutOfService_WhenOpen_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(status: TerminalStatus.Open);
        var result = terminal.TakeOutOfService();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Open_WhenInMaintenance_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister(status: TerminalStatus.Maintenance);
        var result = terminal.Open();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void UnassignOperator_WhenClosed_ReturnsFailure()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var result = terminal.UnassignOperator();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Rename_UpdatesName()
    {
        var terminal = StoreSettingsTestHelpers.CreateCashRegister();
        var result = terminal.Rename(CashRegisterTerminalName.Create("Caixa 2").Value);

        Assert.True(result.IsSuccess);
        Assert.Equal("Caixa 2", terminal.Name.Value);
    }
}
