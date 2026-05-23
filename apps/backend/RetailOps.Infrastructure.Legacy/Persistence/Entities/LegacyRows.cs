namespace RetailOps.Infrastructure.Legacy.Persistence.Entities;

public sealed class LegacyCompanyRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Cpf { get; set; }
    public string? Cnpj { get; set; }
    public string Active { get; set; } = "Sim";
    public DateTime? NextBillingDate { get; set; }
    public decimal MonthlyFee { get; set; }
    public string Trial { get; set; } = "Não";
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

public sealed class LegacyContractRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime SignedDate { get; set; }
}

public sealed class LegacyConfigRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? TrialDays { get; set; }
    public int? BlockDays { get; set; }
    public string? BlockMessage { get; set; }
}

public sealed class LegacyReceivableRow
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public int PersonId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string Paid { get; set; } = "Não";
}
