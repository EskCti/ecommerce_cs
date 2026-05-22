using RetailOps.Identity.Core.Application.Ports;

namespace RetailOps.Identity.Infrastructure.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => BCrypt.Net.BCrypt.HashPassword(plainPassword);

    public bool Verify(string plainPassword, string hash) => BCrypt.Net.BCrypt.Verify(plainPassword, hash);

    public bool IsBcryptHash(string hash) =>
        hash.StartsWith("$2a$", StringComparison.Ordinal)
        || hash.StartsWith("$2b$", StringComparison.Ordinal)
        || hash.StartsWith("$2y$", StringComparison.Ordinal);
}
