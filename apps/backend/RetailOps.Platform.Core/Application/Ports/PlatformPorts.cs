using RetailOps.Platform.Core.Domain.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Platform.Core.Application.Ports;

public interface ICompanyRepository
{
    Task<Company?> FindByIdAsync(int legacyCompanyId, CancellationToken ct = default);
    Task<Company?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<ResultPage<Company>> ListAsync(CompanyListFilter filter, CancellationToken ct = default);
    Task<Result<int>> SaveAsync(Company company, CancellationToken ct = default);
    Task<Result> SaveContractAsync(int legacyCompanyId, Contract contract, CancellationToken ct = default);
}

public sealed record CompanyListFilter(
    string? Name,
    bool? Active,
    bool? Trial,
    int Page,
    int PageSize);

public sealed record ResultPage<T>(IReadOnlyList<T> Items, int Total);

public interface IPlatformConfigRepository
{
    Task<PlatformConfig?> GetGlobalAsync(CancellationToken ct = default);
}

public sealed record ProvisionedAdminUser(Guid UserId, int LegacyUserId);

public interface IUserProvisioningPort
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    Task<Result<ProvisionedAdminUser>> ProvisionTrialAdminAsync(
        int tenantId,
        string name,
        string email,
        string password,
        CancellationToken ct = default);
}

public interface IUserDeactivationPort
{
    Task<Result> DeactivateUsersByTenantAsync(int tenantId, CancellationToken ct = default);
}

public interface ITenantInvoicePort
{
    Task<Result> IssueInvoiceAsync(int tenantId, decimal amount, DateOnly dueDate, CancellationToken ct = default);
}

public interface ITenantSuspendedPublisher
{
    Task PublishAsync(int tenantId, CancellationToken ct = default);
}
