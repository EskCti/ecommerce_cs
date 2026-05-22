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
    public string? Cpf { get; set; }
    public string? PasswordMd5 { get; set; }
    public string? ManagerPinPlain { get; set; }
    public string Level { get; set; } = "Operador";
    public string Active { get; set; } = "Sim";
}

public sealed class LegacyUserPermissionRow
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AccessId { get; set; }
}

public sealed class LegacyAccessRow
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int GroupId { get; set; }
}

public sealed class LegacyProductRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
