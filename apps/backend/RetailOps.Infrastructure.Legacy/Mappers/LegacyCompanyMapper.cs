using RetailOps.Infrastructure.Legacy.Persistence.Entities;

namespace RetailOps.Infrastructure.Legacy.Mappers;

public sealed record LegacyCompanySnapshot(int LegacyId, string Name, bool IsActive);

public static class LegacyCompanyMapper
{
    public static LegacyCompanySnapshot ToSnapshot(LegacyCompanyRow row) =>
        new(row.Id, row.Name, string.Equals(row.Active, "Sim", StringComparison.OrdinalIgnoreCase));
}
