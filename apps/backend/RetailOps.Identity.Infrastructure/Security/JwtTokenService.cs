using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Services;

namespace RetailOps.Identity.Infrastructure.Security;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var key = configuration["Jwt:Key"] ?? "RetailOps_Dev_Signing_Key_Change_In_Production_32chars!";
        var issuer = configuration["Jwt:Issuer"] ?? "RetailOps";
        var audience = configuration["Jwt:Audience"] ?? "RetailOps";
        var minutes = int.TryParse(configuration["Jwt:ExpiresMinutes"], out var m) ? m : 480;

        var expires = DateTime.UtcNow.AddMinutes(minutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("tenant_id", user.TenantId.Value.ToString()),
            new("user_level", user.Level.ToString()),
            new("legacy_user_id", user.LegacyUserId.ToString()),
        };

        if (AuthorizationPolicy.IsPrivileged(user.Level))
        {
            claims.Add(new Claim("permission_keys", "*"));
        }
        else
        {
            foreach (var grant in user.Grants)
                claims.Add(new Claim("permission_keys", grant.PermissionKey.Value));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public Guid? GetUserIdFromPrincipal(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
