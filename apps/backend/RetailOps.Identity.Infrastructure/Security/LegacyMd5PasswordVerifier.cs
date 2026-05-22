using System.Security.Cryptography;
using System.Text;
using RetailOps.Identity.Core.Domain.Services;

namespace RetailOps.Identity.Infrastructure.Security;

public sealed class LegacyMd5PasswordVerifier : ILegacyMd5PasswordVerifier
{
    public bool Verify(string plainPassword, string legacyMd5Hash)
    {
        if (string.IsNullOrWhiteSpace(legacyMd5Hash))
            return false;

        var hash = ComputeMd5(plainPassword);
        return string.Equals(hash, legacyMd5Hash, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeMd5(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
