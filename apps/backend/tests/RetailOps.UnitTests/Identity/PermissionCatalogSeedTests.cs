using RetailOps.Identity.Infrastructure.Seeds;
using Xunit;

namespace RetailOps.UnitTests.Identity;

public class PermissionCatalogSeedTests
{
    [Fact]
    public void Seed_Contains35AccessEntries()
    {
        Assert.Equal(35, PermissionCatalogSeed.Items.Count);
    }

    [Fact]
    public void Seed_ContainsUsuariosPermission()
    {
        Assert.Contains(PermissionCatalogSeed.Items, i => i.Key == "usuarios");
    }
}
