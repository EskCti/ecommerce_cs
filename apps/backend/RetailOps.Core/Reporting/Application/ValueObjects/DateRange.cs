using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Reporting.Application.ValueObjects;

public sealed record DateRange
{
    public DateTime From { get; }
    public DateTime To { get; }

    private DateRange(DateTime from, DateTime to)
    {
        From = from;
        To = to;
    }

    public static Result<DateRange> Create(DateTime from, DateTime to)
    {
        if (from == default || to == default)
            return Result<DateRange>.Failure("Date range is required.");

        var fromDate = from.Date;
        var toDate = to.Date;

        if (fromDate > toDate)
            return Result<DateRange>.Failure("Start date must be before or equal to end date.");

        return Result<DateRange>.Success(new DateRange(fromDate, toDate.AddDays(1).AddTicks(-1)));
    }
}
