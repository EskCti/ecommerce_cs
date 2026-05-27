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
    public string? Description { get; set; }
    public int Stock { get; set; }
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal ProfitMargin { get; set; }
    public int? SupplierLegacyId { get; set; }
    public int? CategoryLegacyId { get; set; }
    public int StockAlertLevel { get; set; }
    public string? Active { get; set; }
    public string? Photo { get; set; }
}

public sealed class LegacyCategoryRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Active { get; set; }
}

public sealed class LegacyGradeDimensionRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int ProductLegacyId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class LegacyGradeOptionRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int DimensionLegacyId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Stock { get; set; }
}

public sealed class LegacyStockEntryRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int ProductLegacyId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? MovementType { get; set; }
}

public sealed class LegacyStockExitRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int ProductLegacyId { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? MovementType { get; set; }
}

public sealed class LegacyGradeMovementDetailRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public int MovementLegacyId { get; set; }
    public int OptionLegacyId { get; set; }
    public int Quantity { get; set; }
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
    public string? NomeSistema { get; set; }
    public string? Contatos { get; set; }
    public string? CnpjSistema { get; set; }
    public string? Endereco { get; set; }
    public string? TipoRel { get; set; }
    public string? TipoDesconto { get; set; }
    public decimal? Comissao { get; set; }
    public string? Token { get; set; }
    public string? FotoRel { get; set; }
}

public sealed class LegacyPaymentMethodRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? Nome { get; set; }
    public decimal? Acrescimo { get; set; }
    public string? Ativo { get; set; }
}

public sealed class LegacyCashRegisterRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? Nome { get; set; }
    public string? Status { get; set; }
    public int? Operador { get; set; }
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

public sealed class LegacyCustomerRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? Nome { get; set; }
    public string? Cpf { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? Ativo { get; set; }
}

public sealed class LegacySupplierRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? Nome { get; set; }
    public string? Pessoa { get; set; }
    public string? Cpf { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? Ativo { get; set; }
}

public sealed class LegacyAttachmentRow
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? Tipo { get; set; }
    public int IdRef { get; set; }
    public string? Nome { get; set; }
    public string? Foto { get; set; }
    public DateTime? DataValidade { get; set; }
}
