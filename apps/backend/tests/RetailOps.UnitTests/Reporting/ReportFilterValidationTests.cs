using RetailOps.Core.Reporting.Application.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.Reporting;

public class ReportFilterValidationTests
{
    [Fact]
    public void DateRange_InvalidFromAfterTo_ReturnsFailure()
    {
        var result = DateRange.Create(new DateTime(2026, 7, 10), new DateTime(2026, 7, 1));

        Assert.True(result.IsFailure);
        Assert.Contains("before or equal", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DateRange_ValidRange_Succeeds()
    {
        var from = new DateTime(2026, 7, 1);
        var to = new DateTime(2026, 7, 10);

        var result = DateRange.Create(from, to);

        Assert.True(result.IsSuccess);
        Assert.Equal(from.Date, result.Value.From.Date);
        Assert.Equal(to.Date, result.Value.To.Date);
    }

    [Fact]
    public void ReportFilter_OptionalFields_ArePreserved()
    {
        var customerId = Guid.Parse("00000000-0000-0000-0006-000000000001");
        var sellerId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var result = ReportFilter.Create(
            new DateTime(2026, 7, 1),
            new DateTime(2026, 7, 31),
            customerId,
            sellerId,
            "Pago");

        Assert.True(result.IsSuccess);
        Assert.Equal(customerId, result.Value.CustomerId);
        Assert.Equal(sellerId, result.Value.SellerId);
        Assert.Equal("Pago", result.Value.PaymentStatus);
    }

    [Fact]
    public void ReportFilter_InvalidDateRange_PropagatesError()
    {
        var result = ReportFilter.Create(new DateTime(2026, 8, 1), new DateTime(2026, 7, 1));

        Assert.True(result.IsFailure);
    }
}
