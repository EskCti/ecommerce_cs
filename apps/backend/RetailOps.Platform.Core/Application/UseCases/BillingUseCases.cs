using RetailOps.Platform.Core.Application.Dtos;
using RetailOps.Platform.Core.Application.Ports;
using RetailOps.Platform.Core.Domain.Enums;
using RetailOps.Platform.Core.Domain.Services;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Platform.Core.Application.UseCases;

public sealed class IssueTenantInvoiceUseCase(
    ICompanyRepository companies,
    ITenantInvoicePort invoices) : IUseCase<IssueInvoiceInDto>
{
    public async Task<Result> Execute(IssueInvoiceInDto input)
    {
        var company = await companies.FindByIdAsync(input.CompanyId);
        if (company is null)
            return Result.Failure("Company not found.");

        return await invoices.IssueInvoiceAsync(input.CompanyId, input.Amount, input.DueDate);
    }
}

public sealed class SuspendOverdueTenantUseCase(
    ICompanyRepository companies,
    IPlatformConfigRepository config,
    ITenantInvoicePort invoices,
    ITenantSuspendedPublisher publisher) : IUseCase<SuspendTenantInDto>
{
    public async Task<Result> Execute(SuspendTenantInDto input)
    {
        var company = await companies.FindByIdAsync(input.CompanyId);
        if (company is null)
            return Result.Failure("Company not found.");

        var platformConfig = await config.GetGlobalAsync();
        if (platformConfig is null)
            return Result.Failure("Platform configuration not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!TenantSuspensionPolicy.ShouldSuspend(company, platformConfig, today)
            && !TrialExpirationPolicy.IsExpired(company, today))
            return Result.Failure("Company is not eligible for suspension.");

        company.Suspend();
        var save = await companies.SaveAsync(company);
        if (save.IsFailure)
            return Result.Failure(save.Error);

        await publisher.PublishAsync(input.CompanyId);
        return Result.Success();
    }
}

public sealed class DeactivateUsersOnTenantSuspendedHandler(IUserDeactivationPort deactivation)
{
    public Task<Result> HandleAsync(int tenantId, CancellationToken ct = default) =>
        deactivation.DeactivateUsersByTenantAsync(tenantId, ct);
}
