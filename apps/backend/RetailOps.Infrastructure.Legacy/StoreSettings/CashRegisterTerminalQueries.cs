using Microsoft.EntityFrameworkCore;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Application.Queries;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.StoreSettings;

public sealed class CashRegisterTerminalQueries(
    ICashRegisterTerminalRepository repository,
    LegacySasDbContext db) : ICashRegisterTerminalQueries
{
    public async Task<Result<IReadOnlyList<CashRegisterTerminalListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        CashRegisterTerminalListFilter? filter = null,
        CancellationToken ct = default)
    {
        var result = await repository.GetByTenantId(tenantId);
        if (result.IsFailure)
            return Result<IReadOnlyList<CashRegisterTerminalListItemDto>>.Failure(result.Error);

        var items = result.Value.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter?.Status)
            && Enum.TryParse<TerminalStatus>(filter.Status, true, out var status))
        {
            items = items.Where(t => t.Status == status);
        }

        if (filter?.AssignedOperatorId is Guid operatorId)
            items = items.Where(t => t.AssignedOperatorId?.Value == operatorId);

        if (!string.IsNullOrWhiteSpace(filter?.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            items = items.Where(t => t.Name.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var page = Math.Max(1, filter?.Page ?? 1);
        var pageSize = Math.Clamp(filter?.PageSize ?? 20, 1, 100);

        var terminals = items
            .OrderBy(t => t.Name.Value)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var operatorNames = await LoadOperatorNamesAsync(terminals, ct);

        var projected = terminals
            .Select(t => CashRegisterTerminalListItemDto.FromDomain(
                t,
                t.AssignedOperatorId is { IsEmpty: false } operatorId
                    && operatorNames.TryGetValue(operatorId.Value, out var name)
                        ? name
                        : null))
            .ToList();

        return Result<IReadOnlyList<CashRegisterTerminalListItemDto>>.Success(projected);
    }

    public async Task<Result<CashRegisterTerminalListItemDto?>> GetByIdAsync(
        TenantId tenantId,
        Guid terminalId,
        CancellationToken ct = default)
    {
        var result = await repository.GetById(terminalId);
        if (result.IsFailure)
            return Result<CashRegisterTerminalListItemDto?>.Failure(result.Error);

        if (result.Value.TenantId.Value != tenantId.Value)
            return Result<CashRegisterTerminalListItemDto?>.Success(null);

        string? operatorName = null;
        if (result.Value.AssignedOperatorId is { IsEmpty: false } operatorId)
        {
            var suffix = operatorId.Value.ToString().Split('-').Last();
            if (int.TryParse(suffix, out var legacyUserId))
            {
                operatorName = await db.Users.AsNoTracking()
                    .Where(u => u.Id == legacyUserId)
                    .Select(u => u.Name)
                    .FirstOrDefaultAsync(ct);
            }
        }

        return Result<CashRegisterTerminalListItemDto?>.Success(
            CashRegisterTerminalListItemDto.FromDomain(result.Value, operatorName));
    }

    private async Task<Dictionary<Guid, string>> LoadOperatorNamesAsync(
        IReadOnlyList<CashRegisterTerminal> terminals,
        CancellationToken ct)
    {
        var legacyUserIds = terminals
            .Where(t => t.AssignedOperatorId is { IsEmpty: false })
            .Select(t =>
            {
                var suffix = t.AssignedOperatorId!.Value.Value.ToString().Split('-').Last();
                return int.TryParse(suffix, out var id) ? id : (int?)null;
            })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (legacyUserIds.Count == 0)
            return [];

        var users = await db.Users.AsNoTracking()
            .Where(u => legacyUserIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Name })
            .ToListAsync(ct);

        return users.ToDictionary(
            u => LegacyStoreSettingsIds.User(u.Id),
            u => u.Name);
    }
}
