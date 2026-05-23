using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Platform.Core.Domain.Entities;
using RetailOps.Platform.Core.Domain.Enums;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Platform.Infrastructure.Legacy;

internal static class LegacyCompanyMapper
{
    internal static Result<Company> ToDomain(LegacyCompanyRow row, IReadOnlyList<LegacyContractRow> contracts)
    {
        var mappedContracts = new List<Contract>();
        foreach (var c in contracts)
        {
            var contract = Contract.Reconstitute(
                ToGuid(c.Id, "0002"),
                c.Id,
                c.Text,
                DateOnly.FromDateTime(c.SignedDate));
            if (contract.IsSuccess)
                mappedContracts.Add(contract.Value);
        }

        return Company.Reconstitute(
            ToGuid(row.Id, "0001"),
            row.Id,
            row.Name,
            row.Phone,
            row.Email,
            row.Cpf,
            row.Cnpj,
            row.Active.Equals("Sim", StringComparison.OrdinalIgnoreCase)
                ? CompanyActiveStatus.Active
                : CompanyActiveStatus.Inactive,
            row.Trial.Equals("Sim", StringComparison.OrdinalIgnoreCase) ? TrialFlag.Yes : TrialFlag.No,
            row.NextBillingDate is null ? null : DateOnly.FromDateTime(row.NextBillingDate.Value),
            row.MonthlyFee,
            mappedContracts);
    }

    internal static void ApplyToRow(Company company, LegacyCompanyRow row)
    {
        row.Name = company.Name;
        row.Phone = company.Phone;
        row.Email = company.Email;
        row.Cpf = company.Cpf;
        row.Cnpj = company.Cnpj;
        row.Active = company.Status == CompanyActiveStatus.Active ? "Sim" : "Não";
        row.Trial = company.Trial == TrialFlag.Yes ? "Sim" : "Não";
        row.NextBillingDate = company.NextBillingDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        row.MonthlyFee = company.MonthlyFee;
    }

    internal static Guid ToGuid(int legacyId, string segment) =>
        Guid.Parse($"00000000-0000-0000-{segment}-{legacyId:D12}");
}
