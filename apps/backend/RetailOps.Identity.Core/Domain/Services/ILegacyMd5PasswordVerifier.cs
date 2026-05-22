namespace RetailOps.Identity.Core.Domain.Services;

public interface ILegacyMd5PasswordVerifier
{
    bool Verify(string plainPassword, string legacyMd5Hash);
}
