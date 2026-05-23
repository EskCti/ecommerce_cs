using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.ValueObjects;

namespace RetailOps.UnitTests.Identity;

internal static class TestUsers
{
    internal static IEnumerable<PermissionGrant> Grants(params string[] keys) =>
        keys.Select(k => PermissionGrant.Create(Guid.NewGuid(), PermissionKey.Create(k).Value));
}
