using RetailOps.Identity.Core.Domain.Enums;
using RetailOps.Identity.Core.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using Email = RetailOps.Shared.Kernel.Domain.ValueObjects.Email;
using TenantId = RetailOps.Shared.Kernel.Domain.ValueObjects.TenantId;

namespace RetailOps.Identity.Core.Domain.Entities;

public sealed class User : Entity
{
    private readonly List<PermissionGrant> _grants = [];

    public int LegacyUserId { get; private set; }
    public TenantId TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Email? Email { get; private set; }
    public Cpf? Cpf { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public ManagerPin? ManagerPinHash { get; private set; }
    public UserLevel Level { get; private set; }
    public ActiveStatus Status { get; private set; }
    public IReadOnlyCollection<PermissionGrant> Grants => _grants.AsReadOnly();

    private User(
        int legacyUserId,
        TenantId tenantId,
        string name,
        Email? email,
        Cpf? cpf,
        PasswordHash passwordHash,
        UserLevel level,
        ActiveStatus status)
    {
        LegacyUserId = legacyUserId;
        TenantId = tenantId;
        Name = name;
        Email = email;
        Cpf = cpf;
        PasswordHash = passwordHash;
        Level = level;
        Status = status;
    }

    public static Result<User> Reconstitute(
        Guid id,
        int legacyUserId,
        TenantId tenantId,
        string name,
        Email? email,
        Cpf? cpf,
        PasswordHash passwordHash,
        ManagerPin? managerPin,
        UserLevel level,
        ActiveStatus status,
        IEnumerable<PermissionGrant> grants)
    {
        var user = new User(legacyUserId, tenantId, name, email, cpf, passwordHash, level, status)
        {
            Id = id,
            ManagerPinHash = managerPin,
        };
        user._grants.AddRange(grants);
        return Result<User>.Success(user);
    }

    public Result AssignPermission(PermissionKey key)
    {
        if (_grants.Any(g => g.PermissionKey.Value == key.Value))
            return Result.Failure("Permission already assigned.");

        _grants.Add(PermissionGrant.Create(Id, key));
        return Result.Success();
    }

    public Result RevokePermission(PermissionKey key)
    {
        var grant = _grants.FirstOrDefault(g => g.PermissionKey.Value == key.Value);
        if (grant is null)
            return Result.Failure("Permission grant not found.");

        _grants.Remove(grant);
        return Result.Success();
    }

    public void Deactivate() => Status = ActiveStatus.Inactive;

    public void UpdatePasswordHash(PasswordHash hash) => PasswordHash = hash;

    public void UpdateManagerPin(ManagerPin pin) => ManagerPinHash = pin;
}
