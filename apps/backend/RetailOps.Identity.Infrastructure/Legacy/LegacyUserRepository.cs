using Microsoft.EntityFrameworkCore;
using RetailOps.Identity.Core.Application.Ports;
using RetailOps.Identity.Core.Domain.Entities;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Identity.Infrastructure.Legacy;

public sealed class LegacyUserRepository(LegacySasDbContext db) : IUserRepository
{
    public async Task<User?> FindByEmailOrCpfAsync(string login, CancellationToken ct = default)
    {
        var normalized = login.Trim().ToLowerInvariant();
        var digits = new string(login.Where(char.IsDigit).ToArray());

        var row = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u =>
                (u.Email != null && u.Email.ToLower() == normalized)
                || (u.Cpf != null && u.Cpf.Replace(".", "").Replace("-", "") == digits),
                ct);

        return row is null ? null : await MapUserAsync(row, ct);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var legacyId = ParseLegacyId(id);
        if (legacyId is null) return null;

        var row = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == legacyId, ct);
        return row is null ? null : await MapUserAsync(row, ct);
    }

    public async Task<IReadOnlyList<User>> ListByTenantAsync(int tenantId, CancellationToken ct = default)
    {
        var rows = await db.Users.AsNoTracking()
            .Where(u => u.CompanyId == tenantId)
            .ToListAsync(ct);

        var users = new List<User>();
        foreach (var row in rows)
        {
            var mapped = await MapUserAsync(row, ct);
            if (mapped is not null)
                users.Add(mapped);
        }

        return users;
    }

    public async Task<Result> SaveAsync(User user, CancellationToken ct = default)
    {
        var row = await db.Users.FirstOrDefaultAsync(u => u.Id == user.LegacyUserId, ct);
        if (row is null)
            return Result.Failure("Legacy user not found.");

        row.PasswordMd5 = user.PasswordHash.Value;
        if (user.ManagerPinHash is not null)
            row.ManagerPinPlain = user.ManagerPinHash.Value.BcryptHash;

        var existing = await db.UserPermissions.Where(p => p.UserId == user.LegacyUserId).ToListAsync(ct);
        db.UserPermissions.RemoveRange(existing);

        var catalog = await db.AccessCatalog.AsNoTracking().ToListAsync(ct);
        foreach (var grant in user.Grants)
        {
            var access = catalog.FirstOrDefault(a =>
                a.Key.Equals(grant.PermissionKey.Value, StringComparison.OrdinalIgnoreCase));
            if (access is null) continue;

            db.UserPermissions.Add(new LegacyUserPermissionRow
            {
                UserId = user.LegacyUserId,
                AccessId = access.Id,
            });
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await db.Users.AsNoTracking()
            .AnyAsync(u => u.Email != null && u.Email.ToLower() == normalized, ct);
    }

    public async Task<Result<(Guid UserId, int LegacyUserId)>> CreateTenantAdminAsync(
        int tenantId,
        string name,
        string email,
        string passwordHash,
        CancellationToken ct = default)
    {
        if (await ExistsByEmailAsync(email, ct))
            return Result<(Guid, int)>.Failure("Email already registered.");

        var row = new LegacyUserRow
        {
            CompanyId = tenantId,
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordMd5 = passwordHash,
            Level = "Administrador",
            Active = "Sim",
        };

        db.Users.Add(row);
        await db.SaveChangesAsync(ct);

        var userId = Guid.Parse($"00000000-0000-0000-0000-{row.Id:D12}");
        return Result<(Guid, int)>.Success((userId, row.Id));
    }

    public async Task<Result> DeactivateByTenantAsync(int tenantId, CancellationToken ct = default)
    {
        var users = await db.Users.Where(u => u.CompanyId == tenantId).ToListAsync(ct);
        foreach (var user in users)
            user.Active = "Não";

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<User?> MapUserAsync(LegacyUserRow row, CancellationToken ct)
    {
        var perms = await (
            from up in db.UserPermissions.AsNoTracking()
            join a in db.AccessCatalog.AsNoTracking() on up.AccessId equals a.Id
            where up.UserId == row.Id
            select new { up.AccessId, a.Key }
        ).ToListAsync(ct);

        var result = LegacyUserMapper.ToDomain(row, perms.Select(p => (p.AccessId, p.Key)).ToList());
        return result.IsSuccess ? result.Value : null;
    }

    private static int? ParseLegacyId(Guid id)
    {
        var suffix = id.ToString().Split('-').Last();
        return int.TryParse(suffix, out var legacyId) ? legacyId : null;
    }
}
