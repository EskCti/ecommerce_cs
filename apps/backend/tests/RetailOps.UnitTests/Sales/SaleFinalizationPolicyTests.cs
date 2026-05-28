using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Services;
using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Sales;

public class SaleFinalizationPolicyTests
{
    [Fact]
    public void Validate_EmptyCart_FailsRn044()
    {
        var session = OpenSession();

        var result = SaleFinalizationPolicy.Validate(session, PaymentTerms.Cash, null, 100m, 50m);

        Assert.True(result.IsFailure);
        Assert.Contains("empty", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_NegativeChange_FailsRn043()
    {
        var session = SessionWithReadyLine();

        var result = SaleFinalizationPolicy.Validate(session, PaymentTerms.Cash, null, 10m, 50m);

        Assert.True(result.IsFailure);
        Assert.Contains("negative", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_CreditWithoutCustomer_FailsRn041()
    {
        var session = SessionWithReadyLine();

        var result = SaleFinalizationPolicy.Validate(session, PaymentTerms.Credit, null, 100m, 50m);

        Assert.True(result.IsFailure);
        Assert.Contains("Customer", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ValidCashSale_Succeeds()
    {
        var session = SessionWithReadyLine();

        var result = SaleFinalizationPolicy.Validate(session, PaymentTerms.Cash, null, 100m, 50m);

        Assert.True(result.IsSuccess);
    }

    private static CashSession OpenSession()
    {
        var tenant = TenantId.Create(1).Value;
        return CashSession.Open(tenant, Guid.NewGuid(), Guid.NewGuid(), 50m, true).Value;
    }

    private static CashSession SessionWithReadyLine()
    {
        var session = OpenSession();
        var line = SaleLine.Create(
            Guid.NewGuid(),
            "789",
            1,
            UnitPrice.Create(50m).Value,
            requiresGrade: false).Value;
        session.AddLine(line);
        return session;
    }
}
