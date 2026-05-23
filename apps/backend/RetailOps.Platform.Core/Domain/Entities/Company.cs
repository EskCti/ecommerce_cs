using RetailOps.Platform.Core.Domain.Enums;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Platform.Core.Domain.Entities;

public sealed class Company : Entity
{
    private readonly List<Contract> _contracts = [];

    public int LegacyCompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Cpf { get; private set; }
    public string? Cnpj { get; private set; }
    public CompanyActiveStatus Status { get; private set; }
    public TrialFlag Trial { get; private set; }
    public DateOnly? NextBillingDate { get; private set; }
    public decimal MonthlyFee { get; private set; }
    public IReadOnlyCollection<Contract> Contracts => _contracts.AsReadOnly();

    private Company(
        int legacyCompanyId,
        string name,
        CompanyActiveStatus status,
        TrialFlag trial,
        DateOnly? nextBillingDate,
        decimal monthlyFee)
    {
        LegacyCompanyId = legacyCompanyId;
        Name = name;
        Status = status;
        Trial = trial;
        NextBillingDate = nextBillingDate;
        MonthlyFee = monthlyFee;
    }

    public static Result<Company> RegisterTrial(
        int legacyCompanyId,
        string name,
        string? phone,
        string? email,
        string? cpf,
        string? cnpj,
        DateOnly dueDate,
        decimal monthlyFee = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Company>.Failure("Company name is required.");

        var company = new Company(
            legacyCompanyId,
            name.Trim(),
            CompanyActiveStatus.Active,
            TrialFlag.Yes,
            dueDate,
            monthlyFee)
        {
            Phone = phone?.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            Cpf = cpf?.Trim(),
            Cnpj = cnpj?.Trim(),
        };

        return Result<Company>.Success(company);
    }

    public static Result<Company> Create(
        int legacyCompanyId,
        string name,
        string? phone,
        string? email,
        string? cpf,
        string? cnpj,
        bool trial,
        DateOnly? nextBillingDate,
        decimal monthlyFee)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Company>.Failure("Company name is required.");

        var company = new Company(
            legacyCompanyId,
            name.Trim(),
            CompanyActiveStatus.Active,
            trial ? TrialFlag.Yes : TrialFlag.No,
            nextBillingDate,
            monthlyFee)
        {
            Phone = phone?.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            Cpf = cpf?.Trim(),
            Cnpj = cnpj?.Trim(),
        };

        return Result<Company>.Success(company);
    }

    public static Result<Company> Reconstitute(
        Guid id,
        int legacyCompanyId,
        string name,
        string? phone,
        string? email,
        string? cpf,
        string? cnpj,
        CompanyActiveStatus status,
        TrialFlag trial,
        DateOnly? nextBillingDate,
        decimal monthlyFee,
        IEnumerable<Contract> contracts)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Company>.Failure("Company name is required.");

        var company = new Company(legacyCompanyId, name.Trim(), status, trial, nextBillingDate, monthlyFee)
        {
            Id = id,
            Phone = phone,
            Email = email,
            Cpf = cpf,
            Cnpj = cnpj,
        };
        company._contracts.AddRange(contracts);
        return Result<Company>.Success(company);
    }

    public Result UpdateDetails(string name, string? phone, string? email, string? cpf, string? cnpj, decimal monthlyFee)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Company name is required.");

        Name = name.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim().ToLowerInvariant();
        Cpf = cpf?.Trim();
        Cnpj = cnpj?.Trim();
        MonthlyFee = monthlyFee;
        return Result.Success();
    }

    public void Suspend() => Status = CompanyActiveStatus.Inactive;

    public void Activate() => Status = CompanyActiveStatus.Active;

    public void UpdateBillingDate(DateOnly date) => NextBillingDate = date;

    public Result AddContract(Contract contract)
    {
        _contracts.Add(contract);
        return Result.Success();
    }

    public Result SaveContract(Contract contract)
    {
        var existing = _contracts.FirstOrDefault(c => c.LegacyContractId == contract.LegacyContractId);
        if (existing is null)
            return AddContract(contract);

        return existing.Update(contract.Text, contract.SignedDate);
    }
}
