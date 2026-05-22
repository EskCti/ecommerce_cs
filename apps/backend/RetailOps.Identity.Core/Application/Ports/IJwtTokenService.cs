using RetailOps.Identity.Core.Domain.Entities;

namespace RetailOps.Identity.Core.Application.Ports;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
    Guid? GetUserIdFromPrincipal(System.Security.Claims.ClaimsPrincipal principal);
}
