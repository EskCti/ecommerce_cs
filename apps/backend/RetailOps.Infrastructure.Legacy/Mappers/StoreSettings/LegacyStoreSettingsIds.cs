namespace RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;

internal static class LegacyStoreSettingsIds
{
    internal static Guid StoreConfig(int tenantId) =>
        Guid.Parse($"00000000-0000-0000-0003-{tenantId:D12}");

    internal static Guid PaymentMethod(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0004-{legacyId:D12}");

    internal static Guid CashRegisterTerminal(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0005-{legacyId:D12}");

    internal static Guid User(int legacyUserId) =>
        Guid.Parse($"00000000-0000-0000-0000-{legacyUserId:D12}");

    internal static int? ParseLegacyId(Guid id, string segment)
    {
        var parts = id.ToString().Split('-');
        if (parts.Length != 5 || parts[3] != segment)
            return null;

        return int.TryParse(parts[4], out var legacyId) ? legacyId : null;
    }
}
