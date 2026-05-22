using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Email = RetailOps.Shared.Kernel.Domain.ValueObjects.Email;

namespace RetailOps.Identity.Infrastructure.Legacy;

internal static class LegacyUserMapper
{
    public static Result<User> ToDomain(
        LegacyUserRow row,
        IReadOnlyList<(int AccessId, string Key)> permissions)
    {
        var tenant = TenantId.Create(row.CompanyId);
        if (tenant.IsFailure)
            return Result<User>.Failure(tenant.Error);

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(row.Email))
        {
            var emailResult = Email.Create(row.Email);
            if (emailResult.IsFailure)
                return Result<User>.Failure(emailResult.Error);
            email = emailResult.Value;
        }

        Cpf? cpf = null;
        if (!string.IsNullOrWhiteSpace(row.Cpf))
        {
            var cpfResult = Cpf.Create(row.Cpf);
            if (cpfResult.IsSuccess)
                cpf = cpfResult.Value;
        }

        var hashValue = row.PasswordMd5 ?? string.Empty;
        var passwordHash = PasswordHash.CreateBcrypt(hashValue);
        if (passwordHash.IsFailure)
            passwordHash = PasswordHash.CreateBcrypt("invalid");

        ManagerPin? managerPin = null;
        if (!string.IsNullOrWhiteSpace(row.ManagerPinPlain) && row.ManagerPinPlain.StartsWith("$2"))
        {
            var pin = ManagerPin.FromBcrypt(row.ManagerPinPlain);
            if (pin.IsSuccess)
                managerPin = pin.Value;
        }

        var grants = permissions
            .Select(p =>
            {
                var key = PermissionKey.Create(p.Key);
                return key.IsSuccess ? PermissionGrant.Create(StableGuid(row.Id), key.Value) : null;
            })
            .Where(g => g is not null)
            .Cast<PermissionGrant>()
            .ToList();

        return User.Reconstitute(
            StableGuid(row.Id),
            row.Id,
            tenant.Value,
            row.Name,
            email,
            cpf,
            passwordHash.Value,
            managerPin,
            MapLevel(row.Level),
            row.Active.Equals("Sim", StringComparison.OrdinalIgnoreCase) ? ActiveStatus.Active : ActiveStatus.Inactive,
            grants);
    }

    public static Guid StableGuid(int legacyUserId) =>
        Guid.Parse($"00000000-0000-0000-0000-{legacyUserId:D12}");

    public static UserLevel MapLevel(string nivel) => nivel.Trim().ToUpperInvariant() switch
    {
        "SAS" => UserLevel.Sas,
        "ADMINISTRADOR" => UserLevel.Administrador,
        "GERENTE" => UserLevel.Gerente,
        "VENDEDOR" => UserLevel.Vendedor,
        _ => UserLevel.Operador,
    };
}
