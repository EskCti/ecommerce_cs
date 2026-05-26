using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public record ReportFormat
{
    public string Value { get; }

    private ReportFormat(string value) => Value = value;

    public static Result<ReportFormat> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<ReportFormat>.Failure("Report format is required.");

        var trimmed = value.Trim().ToUpperInvariant();
        
        var validFormats = new[] { "PDF", "EXCEL", "CSV", "HTML" };
        
        if (!validFormats.Contains(trimmed))
            return Result<ReportFormat>.Failure($"Invalid report format. Valid formats are: {string.Join(", ", validFormats)}.");

        return Result<ReportFormat>.Success(new ReportFormat(trimmed));
    }

    public static implicit operator string(ReportFormat reportFormat) => reportFormat.Value;

    public string ToLegacyValue()
    {
        return Value switch
        {
            "PDF" => "P",
            "EXCEL" => "E",
            "CSV" => "C",
            "HTML" => "H",
            _ => "P"
        };
    }

    public static ReportFormat FromLegacyValue(string legacyValue)
    {
        var format = legacyValue?.ToUpperInvariant() switch
        {
            "P" => "PDF",
            "E" => "EXCEL",
            "C" => "CSV",
            "H" => "HTML",
            _ => "PDF"
        };

        return new ReportFormat(format);
    }
}