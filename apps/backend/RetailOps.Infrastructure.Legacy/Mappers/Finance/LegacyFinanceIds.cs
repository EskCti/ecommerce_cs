namespace RetailOps.Infrastructure.Legacy.Mappers.Finance;

internal static class LegacyFinanceIds
{
    internal static Guid Receivable(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0019-{legacyId:D12}");

    internal static Guid Payable(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0020-{legacyId:D12}");

    internal static Guid Commission(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0021-{legacyId:D12}");

    internal static int? ParseLegacyId(Guid id, string segment)
    {
        var parts = id.ToString().Split('-');
        if (parts.Length != 5 || parts[3] != segment)
            return null;

        return int.TryParse(parts[4], out var legacyId) ? legacyId : null;
    }

    internal static int? ParseReceivableLegacyId(Guid id) => ParseLegacyId(id, "0019");
    internal static int? ParsePayableLegacyId(Guid id) => ParseLegacyId(id, "0020");
    internal static int? ParseCommissionLegacyId(Guid id) => ParseLegacyId(id, "0021");
}
