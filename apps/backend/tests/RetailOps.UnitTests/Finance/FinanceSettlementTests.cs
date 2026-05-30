using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.Services;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Finance;

public class ReceivableSettlementServiceTests
{
    private readonly ReceivableSettlementService _service = new();

    [Fact]
    public void Settle_OpenReceivable_Succeeds()
    {
        var tenant = TenantId.Create(1).Value;
        var receivable = Receivable.CreateManual(
            tenant,
            "Test",
            Money.Create(100).Value,
            DueDate.Create(DateTime.UtcNow.AddDays(7)).Value).Value;

        var result = _service.Settle(receivable, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Settled, receivable.Status);
    }

    [Fact]
    public void Settle_AlreadySettled_Fails()
    {
        var tenant = TenantId.Create(1).Value;
        var receivable = Receivable.CreateManual(
            tenant,
            "Test",
            Money.Create(100).Value,
            DueDate.Create(DateTime.UtcNow.AddDays(7)).Value).Value;

        _service.Settle(receivable, DateTime.UtcNow);
        var result = _service.Settle(receivable, DateTime.UtcNow);

        Assert.True(result.IsFailure);
    }
}

public class PayableSettlementServiceTests
{
    private readonly PayableSettlementService _service = new();

    [Fact]
    public void CreatePurchase_FutureDueDate_StaysOpen()
    {
        var tenant = TenantId.Create(1).Value;
        var due = DueDate.Create(DateTime.UtcNow.AddDays(10)).Value;
        var payable = Payable.CreatePurchase(
            tenant,
            "Compra",
            Money.Create(50).Value,
            due,
            Guid.NewGuid(),
            supplierLegacyId: 1,
            DateTime.UtcNow).Value;

        Assert.Equal(PaymentStatus.Open, payable.Status);
    }

    [Fact]
    public void Settle_OpenPayable_Succeeds()
    {
        var tenant = TenantId.Create(1).Value;
        var payable = Payable.CreateExpense(
            tenant,
            "Despesa",
            Money.Create(80).Value,
            DueDate.Create(DateTime.UtcNow.AddDays(3)).Value).Value;

        var result = _service.Settle(payable, DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Settled, payable.Status);
    }
}

public class CommissionSettlementServiceTests
{
    private readonly CommissionSettlementService _service = new();

    [Fact]
    public void PayCommission_CreatesPaymentPayable()
    {
        var tenant = TenantId.Create(1).Value;
        var commission = Commission.Create(
            tenant,
            Guid.NewGuid(),
            sellerLegacyId: 2,
            Money.Create(25).Value).Value;
        commission.SyncIdentity(Guid.NewGuid());

        var payableResult = _service.CreatePaymentPayable(commission, DateTime.UtcNow);

        Assert.True(payableResult.IsSuccess);
        Assert.Equal(AccountType.CommissionPayment, payableResult.Value.Type);
    }
}
