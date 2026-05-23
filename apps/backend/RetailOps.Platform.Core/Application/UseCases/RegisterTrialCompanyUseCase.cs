using RetailOps.Platform.Core.Application.Dtos;
using RetailOps.Platform.Core.Application.Ports;
using RetailOps.Platform.Core.Domain.Entities;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.Transactions;

namespace RetailOps.Platform.Core.Application.UseCases;

public sealed class RegisterTrialCompanyUseCase(
    ICompanyRepository companies,
    IPlatformConfigRepository config,
    IUserProvisioningPort userProvisioning,
    ITransactionManager transactions) : IUseCase<RegisterTrialInDto, RegisterTrialOutDto>
{
    public async Task<Result<RegisterTrialOutDto>> Execute(RegisterTrialInDto input)
    {
        var normalizedEmail = input.AdminEmail.Trim().ToLowerInvariant();
        if (await companies.FindByEmailAsync(normalizedEmail) is not null
            || await userProvisioning.EmailExistsAsync(normalizedEmail))
            return Result<RegisterTrialOutDto>.Failure("Email already registered.");

        var platformConfig = await config.GetGlobalAsync();
        var trialDays = platformConfig?.TrialDays ?? 7;
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(trialDays);

        return await transactions.RunInTransactionAsync(async () =>
        {
            var companyResult = Company.RegisterTrial(
                0,
                input.CompanyName,
                input.Phone,
                normalizedEmail,
                input.Cpf,
                input.Cnpj,
                dueDate);

            if (companyResult.IsFailure)
                return Result<RegisterTrialOutDto>.Failure(companyResult.Error);

            var save = await companies.SaveAsync(companyResult.Value);
            if (save.IsFailure)
                return Result<RegisterTrialOutDto>.Failure(save.Error);

            var tenantId = save.Value;
            var provision = await userProvisioning.ProvisionTrialAdminAsync(
                tenantId,
                input.AdminName,
                normalizedEmail,
                input.AdminPassword);

            if (provision.IsFailure)
                return Result<RegisterTrialOutDto>.Failure(provision.Error);

            return Result<RegisterTrialOutDto>.Success(new RegisterTrialOutDto(
                tenantId,
                companyResult.Value.Id,
                provision.Value.UserId));
        });
    }
}
