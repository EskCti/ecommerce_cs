using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Domain.ValueObjects;

public record ScanQuantityPrefix
{
    public const string PrefixMarker = "2*";

    public int Quantity { get; }
    public string Barcode { get; }

    private ScanQuantityPrefix(int quantity, string barcode)
    {
        Quantity = quantity;
        Barcode = barcode;
    }

    public static Result<ScanQuantityPrefix> Parse(string scannedValue)
    {
        if (string.IsNullOrWhiteSpace(scannedValue))
            return Result<ScanQuantityPrefix>.Failure("Scan value is required.");

        var trimmed = scannedValue.Trim();

        if (trimmed.StartsWith(PrefixMarker, StringComparison.Ordinal))
        {
            var barcode = trimmed[PrefixMarker.Length..].Trim();
            if (string.IsNullOrWhiteSpace(barcode))
                return Result<ScanQuantityPrefix>.Failure("Barcode is required after quantity prefix.");

            return Result<ScanQuantityPrefix>.Success(new ScanQuantityPrefix(2, barcode));
        }

        return Result<ScanQuantityPrefix>.Success(new ScanQuantityPrefix(1, trimmed));
    }
}
