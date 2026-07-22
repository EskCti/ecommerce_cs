using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Application.Queries;
using RetailOps.Identity.Core.Application.UseCases;
using RetailOps.Identity.Core.Domain.Services;
using RetailOps.Identity.Infrastructure.Authorization;
using RetailOps.Identity.Infrastructure.Legacy;
using RetailOps.Identity.Infrastructure.Platform;
using RetailOps.Identity.Infrastructure.Queries;
using RetailOps.Identity.Infrastructure.Security;
using RetailOps.Platform.Core.Application.Ports;

namespace RetailOps.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, LegacyUserRepository>();
        services.AddScoped<IPermissionCatalogQuery, PermissionCatalogQuery>();

        services.AddScoped<AuthenticateUserUseCase>();
        services.AddScoped<AssignPermissionsUseCase>();
        services.AddScoped<VerifyManagerPinUseCase>();
        services.AddScoped<ListUsersByTenantQuery>();

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, SasLevelAuthorizationHandler>();

        var key = configuration["Jwt:Key"] ?? "RetailOps_Dev_Signing_Key_Change_In_Production_32chars!";
        var issuer = configuration["Jwt:Issuer"] ?? "RetailOps";
        var audience = configuration["Jwt:Audience"] ?? "RetailOps";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("Permission:usuarios", p => p.Requirements.Add(new PermissionRequirement("usuarios")))
            .AddPolicy("SasOnly", p => p.Requirements.Add(new SasLevelRequirement()));

        services.AddScoped<IUserProvisioningPort, UserProvisioningAdapter>();
        services.AddScoped<IUserDeactivationPort, UserDeactivationAdapter>();

        return services;
    }
}
