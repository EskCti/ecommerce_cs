using RetailOps.Platform.Core.Application.Dtos;
using RetailOps.Platform.Core.Application.Ports;
using RetailOps.Platform.Core.Domain.Enums;

namespace RetailOps.Platform.Core.Application.Queries;

public sealed class ListCompaniesQuery(ICompanyRepository companies)
{
    public async Task<ResultPage<CompanyListItemDto>> Execute(
        string? name,
        bool? active,
        bool? trial,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var filter = new CompanyListFilter(name, active, trial, page, pageSize);
        var pageResult = await companies.ListAsync(filter, ct);

        var items = pageResult.Items.Select(c => new CompanyListItemDto(
            c.LegacyCompanyId,
            c.Id,
            c.Name,
            c.Email,
            c.Status == CompanyActiveStatus.Active,
            c.Trial == TrialFlag.Yes,
            c.NextBillingDate,
            c.MonthlyFee)).ToList();

        return new ResultPage<CompanyListItemDto>(items, pageResult.Total);
    }
}

public sealed class GetCompanyByIdQuery(ICompanyRepository companies)
{
    public async Task<CompanyDetailDto?> Execute(int companyId, CancellationToken ct = default)
    {
        var company = await companies.FindByIdAsync(companyId, ct);
        if (company is null) return null;

        return new CompanyDetailDto(
            company.LegacyCompanyId,
            company.Id,
            company.Name,
            company.Phone,
            company.Email,
            company.Cpf,
            company.Cnpj,
            company.Status == CompanyActiveStatus.Active,
            company.Trial == TrialFlag.Yes,
            company.NextBillingDate,
            company.MonthlyFee,
            company.Contracts.Select(c => new ContractItemDto(c.LegacyContractId, c.Text, c.SignedDate)).ToList());
    }
}
