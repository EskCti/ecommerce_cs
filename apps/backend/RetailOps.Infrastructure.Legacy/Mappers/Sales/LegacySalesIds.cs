namespace RetailOps.Infrastructure.Legacy.Mappers.Sales;

internal static class LegacySalesIds
{
    internal static Guid CashSession(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0015-{legacyId:D12}");

    internal static Guid Sale(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0016-{legacyId:D12}");

    internal static Guid CartLine(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0017-{legacyId:D12}");

    internal static Guid Withdrawal(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0018-{legacyId:D12}");

    internal static int? ParseLegacyId(Guid id, string segment)
    {
        var parts = id.ToString().Split('-');
        if (parts.Length != 5 || parts[3] != segment)
            return null;

        return int.TryParse(parts[4], out var legacyId) ? legacyId : null;
    }

    internal static int? ParseCashSessionLegacyId(Guid id) => ParseLegacyId(id, "0015");
    internal static int? ParseSaleLegacyId(Guid id) => ParseLegacyId(id, "0016");
    internal static int? ParseCartLineLegacyId(Guid id) => ParseLegacyId(id, "0017");
    internal static int? ParseWithdrawalLegacyId(Guid id) => ParseLegacyId(id, "0018");

    internal static int? ParseTerminalLegacyId(Guid terminalId) =>
        Mappers.StoreSettings.LegacyStoreSettingsIds.ParseLegacyId(terminalId, "0005");

    internal static int? ParseOperatorLegacyId(Guid operatorUserId) =>
        Mappers.StoreSettings.LegacyStoreSettingsIds.ParseLegacyId(operatorUserId, "0000");

    internal static int? ParseProductLegacyId(Guid productId) =>
        Mappers.Catalog.LegacyCatalogIds.ParseLegacyId(productId, "0009");

    internal static int? ParseCustomerLegacyId(Guid? customerId) =>
        customerId is null ? null : Mappers.Crm.LegacyCrmIds.ParseLegacyId(customerId.Value, "0006");

    internal static int? ParsePaymentMethodLegacyId(Guid paymentMethodId) =>
        Mappers.StoreSettings.LegacyStoreSettingsIds.ParseLegacyId(paymentMethodId, "0004");
}
