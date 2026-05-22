namespace RetailOps.Identity.Core.Application.Ports;

public interface IPasswordHasher
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string hash);
    bool IsBcryptHash(string hash);
}
