namespace RetailOps.Platform.Core.Application.Dtos;

public sealed record RegisterTrialInDto(
    string CompanyName,
    string AdminName,
    string AdminEmail,
    string AdminPassword,
    string? Phone,
    string? Cpf,
    string? Cnpj);

public sealed record RegisterTrialOutDto(int TenantId, Guid CompanyId, Guid AdminUserId);

public sealed record CreateCompanyInDto(
    string Name,
    string? Phone,
    string? Email,
    string? Cpf,
    string? Cnpj,
    decimal MonthlyFee,
    bool Trial,
    DateOnly? NextBillingDate);

public sealed record UpdateCompanyInDto(
    int CompanyId,
    string Name,
    string? Phone,
    string? Email,
    string? Cpf,
    string? Cnpj,
    decimal MonthlyFee,
    bool Active);

public sealed record SaveContractInDto(int CompanyId, int? ContractId, string Text, DateOnly SignedDate);

public sealed record CompanyListItemDto(
    int Id,
    Guid CompanyId,
    string Name,
    string? Email,
    bool Active,
    bool Trial,
    DateOnly? NextBillingDate,
    decimal MonthlyFee);

public sealed record CompanyDetailDto(
    int Id,
    Guid CompanyId,
    string Name,
    string? Phone,
    string? Email,
    string? Cpf,
    string? Cnpj,
    bool Active,
    bool Trial,
    DateOnly? NextBillingDate,
    decimal MonthlyFee,
    IReadOnlyList<ContractItemDto> Contracts);

public sealed record ContractItemDto(int Id, string Text, DateOnly SignedDate);

public sealed record IssueInvoiceInDto(int CompanyId, decimal Amount, DateOnly DueDate);

public sealed record SuspendTenantInDto(int CompanyId);
