namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public enum TerminalStatus
{
    Closed = 0,
    Open = 1,
    Maintenance = 2,
    OutOfService = 3
}

public static class TerminalStatusExtensions
{
    public static string ToLegacyValue(this TerminalStatus status) =>
        status == TerminalStatus.Open ? "Aberto" : "Fechado";

    public static TerminalStatus FromLegacyValue(string? legacyValue) =>
        legacyValue?.Trim().Equals("Aberto", StringComparison.OrdinalIgnoreCase) == true
            ? TerminalStatus.Open
            : TerminalStatus.Closed;
}