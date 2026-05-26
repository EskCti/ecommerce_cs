namespace RetailOps.Infrastructure.Legacy.Mappers.Crm;

internal static class LegacyCrmIds
{
    internal static Guid Customer(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0006-{legacyId:D12}");

    internal static Guid Supplier(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0007-{legacyId:D12}");

    internal static Guid Attachment(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0008-{legacyId:D12}");

    internal static int? ParseLegacyId(Guid id, string segment)
    {
        var parts = id.ToString().Split('-');
        if (parts.Length != 5 || parts[2] != segment)
            return null;

        return int.TryParse(parts[4], out var legacyId) ? legacyId : null;
    }
}
