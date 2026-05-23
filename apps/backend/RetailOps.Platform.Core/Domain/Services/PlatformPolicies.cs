using RetailOps.Platform.Core.Domain.Entities;
using RetailOps.Platform.Core.Domain.Enums;

namespace RetailOps.Platform.Core.Domain.Services;

public static class TrialExpirationPolicy
{
    public static bool IsExpired(Company company, DateOnly today) =>
        company.Trial == TrialFlag.Yes
        && company.NextBillingDate is not null
        && company.NextBillingDate.Value < today;
}

public static class TenantBillingPolicy
{
    public static bool IsOverdue(Company company, DateOnly today) =>
        company.NextBillingDate is not null
        && company.NextBillingDate.Value < today;
}

public static class TenantSuspensionPolicy
{
    public static bool ShouldSuspend(Company company, PlatformConfig config, DateOnly today)
    {
        if (company.Status == CompanyActiveStatus.Inactive)
            return false;

        if (company.NextBillingDate is null)
            return false;

        var overdueDays = today.DayNumber - company.NextBillingDate.Value.DayNumber;
        return overdueDays >= config.BlockDays;
    }

    public static string BlockMessage(PlatformConfig config) => config.BlockMessage;
}
