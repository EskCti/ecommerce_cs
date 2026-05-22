using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Xunit;

namespace RetailOps.UnitTests.SharedKernel;

public class TenantIdTests
{
    [Fact]
    public void Create_WithZero_ReturnsPlatformScope()
    {
        var result = TenantId.Create(0);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsPlatform);
    }

    [Fact]
    public void Create_WithNegative_ReturnsFailure()
    {
        var result = TenantId.Create(-1);

        Assert.True(result.IsFailure);
    }
}
