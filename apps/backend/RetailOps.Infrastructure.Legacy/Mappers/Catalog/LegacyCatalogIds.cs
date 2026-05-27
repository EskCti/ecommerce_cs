namespace RetailOps.Infrastructure.Legacy.Mappers.Catalog;

internal static class LegacyCatalogIds
{
    internal static Guid Product(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0009-{legacyId:D12}");

    internal static Guid Category(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0010-{legacyId:D12}");

    internal static Guid GradeDimension(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0011-{legacyId:D12}");

    internal static Guid GradeOption(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0012-{legacyId:D12}");

    internal static Guid StockEntry(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0013-{legacyId:D12}");

    internal static Guid StockExit(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0014-{legacyId:D12}");

    internal static int? ParseLegacyId(Guid id, string segment)
    {
        var parts = id.ToString().Split('-');
        if (parts.Length != 5 || parts[3] != segment)
            return null;

        return int.TryParse(parts[4], out var legacyId) ? legacyId : null;
    }

    internal static int? ParseCategoryLegacyId(Guid categoryId) =>
        ParseLegacyId(categoryId, "0010");
}
