namespace RetailOps.Infrastructure.Legacy.Persistence.Entities;

public sealed class LegacyCompanyRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Active { get; set; } = "Sim";
}

public sealed class LegacyUserRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Active { get; set; } = "Sim";
}

public sealed class LegacyProductRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
