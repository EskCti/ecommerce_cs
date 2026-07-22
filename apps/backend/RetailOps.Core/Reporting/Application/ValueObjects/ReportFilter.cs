using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Reporting.Application.ValueObjects;

public sealed record ReportFilter
{
    public DateRange DateRange { get; }
    public Guid? CustomerId { get; }
    public Guid? SellerId { get; }
    public string? PaymentStatus { get; }

    private ReportFilter(DateRange dateRange, Guid? customerId, Guid? sellerId, string? paymentStatus)
    {
        DateRange = dateRange;
        CustomerId = customerId;
        SellerId = sellerId;
        PaymentStatus = paymentStatus;
    }

    public static Result<ReportFilter> Create(
        DateTime from,
        DateTime to,
        Guid? customerId = null,
        Guid? sellerId = null,
        string? paymentStatus = null)
    {
        var rangeResult = DateRange.Create(from, to);
        if (rangeResult.IsFailure)
            return Result<ReportFilter>.Failure(rangeResult.Error);

        return Result<ReportFilter>.Success(new ReportFilter(
            rangeResult.Value,
            customerId,
            sellerId,
            string.IsNullOrWhiteSpace(paymentStatus) ? null : paymentStatus.Trim()));
    }

    public static Result<ReportFilter> WithoutDateRange(
        Guid? customerId = null,
        Guid? sellerId = null,
        string? paymentStatus = null)
    {
        var today = DateTime.UtcNow.Date;
        var rangeResult = DateRange.Create(today, today);
        if (rangeResult.IsFailure)
            return Result<ReportFilter>.Failure(rangeResult.Error);

        return Result<ReportFilter>.Success(new ReportFilter(
            rangeResult.Value,
            customerId,
            sellerId,
            string.IsNullOrWhiteSpace(paymentStatus) ? null : paymentStatus.Trim()));
    }
}
