using System.Text.RegularExpressions;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Notifications.Domain.ValueObjects;

public sealed record PhoneNumber
{
    private static readonly Regex DigitsOnly = new(@"\D", RegexOptions.Compiled);

    public string E164 { get; }

    private PhoneNumber(string e164) => E164 = e164;

    public static Result<PhoneNumber> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Result<PhoneNumber>.Failure("Phone number is required.");

        var digits = DigitsOnly.Replace(raw, "");
        if (digits.Length is < 10 or > 13)
            return Result<PhoneNumber>.Failure("Phone number must contain 10 to 13 digits.");

        var e164 = digits.StartsWith("55", StringComparison.Ordinal) ? $"+{digits}" : $"+55{digits}";
        return Result<PhoneNumber>.Success(new PhoneNumber(e164));
    }

    public override string ToString() => E164;
}
