using RetailOps.Platform.Core.Domain.Entities;
using RetailOps.Platform.Core.Domain.Enums;
using RetailOps.Platform.Core.Domain.Services;
using Xunit;

namespace RetailOps.UnitTests.Platform;

public class PlatformPolicyTests
{
    [Fact]
    public void TrialExpirationPolicy_FlagsExpiredTrial()
    {
        var company = Company.RegisterTrial(
            1, "Loja", null, "a@b.com", null, null,
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1)).Value;

        Assert.True(TrialExpirationPolicy.IsExpired(company, DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    public void TenantSuspensionPolicy_BlocksAfterGracePeriod()
    {
        var config = PlatformConfig.Create(7, 3, "Bloqueado").Value;
        var company = Company.Create(
            1, "Loja", null, null, null, null, false,
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5), 99).Value;

        Assert.True(TenantSuspensionPolicy.ShouldSuspend(company, config, DateOnly.FromDateTime(DateTime.UtcNow)));
    }
}
