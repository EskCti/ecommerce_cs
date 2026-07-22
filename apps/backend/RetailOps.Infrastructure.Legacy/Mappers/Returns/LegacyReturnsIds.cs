namespace RetailOps.Infrastructure.Legacy.Mappers.Returns;

internal static class LegacyReturnsIds
{
    internal static Guid Exchange(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0022-{legacyId:D12}");

    internal static int? ParseLegacyId(Guid id, string segment)
    {
        var parts = id.ToString().Split('-');
        if (parts.Length != 5 || parts[3] != segment)
            return null;

        return int.TryParse(parts[4], out var legacyId) ? legacyId : null;
    }

    internal static int? ParseExchangeLegacyId(Guid id) => ParseLegacyId(id, "0022");
}

internal static class LegacyExchangeMovementTypes
{
    internal const string ExchangeIn = "Troca Entrada";
    internal const string ExchangeOut = "Troca Saída";
}
