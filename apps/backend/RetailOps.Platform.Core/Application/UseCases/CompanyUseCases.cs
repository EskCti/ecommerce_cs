using RetailOps.Platform.Core.Application.Dtos;
using RetailOps.Platform.Core.Application.Ports;
using RetailOps.Platform.Core.Domain.Entities;
using RetailOps.Platform.Core.Domain.Enums;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Platform.Core.Application.UseCases;

public sealed class CreateCompanyUseCase(ICompanyRepository companies) : IUseCase<CreateCompanyInDto, CompanyDetailDto>
{
    public async Task<Result<CompanyDetailDto>> Execute(CreateCompanyInDto input)
    {
        var companyResult = Company.Create(
            0,
            input.Name,
            input.Phone,
            input.Email,
            input.Cpf,
            input.Cnpj,
            input.Trial,
            input.NextBillingDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            input.MonthlyFee);

        if (companyResult.IsFailure)
            return Result<CompanyDetailDto>.Failure(companyResult.Error);

        var save = await companies.SaveAsync(companyResult.Value);
        if (save.IsFailure)
            return Result<CompanyDetailDto>.Failure(save.Error);

        return Result<CompanyDetailDto>.Success(MapDetail(companyResult.Value, save.Value));
    }

    internal static CompanyDetailDto MapDetail(Company company, int legacyId) =>
        new(
            legacyId,
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

public sealed class UpdateCompanyUseCase(ICompanyRepository companies) : IUseCase<UpdateCompanyInDto, CompanyDetailDto>
{
    public async Task<Result<CompanyDetailDto>> Execute(UpdateCompanyInDto input)
    {
        var company = await companies.FindByIdAsync(input.CompanyId);
        if (company is null)
            return Result<CompanyDetailDto>.Failure("Company not found.");

        var update = company.UpdateDetails(input.Name, input.Phone, input.Email, input.Cpf, input.Cnpj, input.MonthlyFee);
        if (update.IsFailure)
            return Result<CompanyDetailDto>.Failure(update.Error);

        if (input.Active)
            company.Activate();
        else
            company.Suspend();

        var save = await companies.SaveAsync(company);
        if (save.IsFailure)
            return Result<CompanyDetailDto>.Failure(save.Error);

        return Result<CompanyDetailDto>.Success(CreateCompanyUseCase.MapDetail(company, input.CompanyId));
    }
}

public sealed class SaveContractUseCase(ICompanyRepository companies) : IUseCase<SaveContractInDto, ContractItemDto>
{
    public async Task<Result<ContractItemDto>> Execute(SaveContractInDto input)
    {
        var company = await companies.FindByIdAsync(input.CompanyId);
        if (company is null)
            return Result<ContractItemDto>.Failure("Company not found.");

        var contractId = input.ContractId ?? 0;
        var contractResult = Contract.Create(contractId, input.Text, input.SignedDate);
        if (contractResult.IsFailure)
            return Result<ContractItemDto>.Failure(contractResult.Error);

        var save = await companies.SaveContractAsync(input.CompanyId, contractResult.Value);
        if (save.IsFailure)
            return Result<ContractItemDto>.Failure(save.Error);

        return Result<ContractItemDto>.Success(new ContractItemDto(
            contractResult.Value.LegacyContractId,
            contractResult.Value.Text,
            contractResult.Value.SignedDate));
    }
}
