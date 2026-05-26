using Microsoft.EntityFrameworkCore;
using RetailOps.Core.StoreSettings.Application.Ports;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.StoreSettings;

public sealed class LegacyCashRegisterTerminalRepository(
    IStoreSettingsLegacyPort legacyPort,
    LegacySasDbContext db) : ICashRegisterTerminalRepository
{
    public async Task<Result<CashRegisterTerminal>> GetById(Guid id)
    {
        var legacyId = LegacyStoreSettingsIds.ParseLegacyId(id, "0005");
        if (legacyId is null)
            return Result<CashRegisterTerminal>.Failure("Invalid cash register terminal id.");

        var row = await db.CashRegisters.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == legacyId);

        if (row is null)
            return Result<CashRegisterTerminal>.Failure("Cash register terminal not found.");

        var mapped = LegacyCashRegisterTerminalMapper.ToDomain(row);
        return mapped.IsFailure
            ? Result<CashRegisterTerminal>.Failure(mapped.Error)
            : Result<CashRegisterTerminal>.Success(mapped.Value);
    }

    public async Task<Result<IEnumerable<CashRegisterTerminal>>> GetByTenantId(TenantId tenantId)
    {
        var result = await legacyPort.GetCashRegisterTerminalsFromLegacyAsync(tenantId);
        if (result.IsFailure)
            return Result<IEnumerable<CashRegisterTerminal>>.Failure(result.Error);

        return Result<IEnumerable<CashRegisterTerminal>>.Success(result.Value);
    }

    public async Task<Result<IEnumerable<CashRegisterTerminal>>> GetByStatus(TenantId tenantId, TerminalStatus status)
    {
        var listResult = await GetByTenantId(tenantId);
        if (listResult.IsFailure)
            return Result<IEnumerable<CashRegisterTerminal>>.Failure(listResult.Error);

        return Result<IEnumerable<CashRegisterTerminal>>.Success(
            listResult.Value.Where(t => t.Status == status));
    }

    public async Task<Result<CashRegisterTerminal>> GetByName(TenantId tenantId, CashRegisterTerminalName name)
    {
        var listResult = await GetByTenantId(tenantId);
        if (listResult.IsFailure)
            return Result<CashRegisterTerminal>.Failure(listResult.Error);

        var match = listResult.Value.FirstOrDefault(t =>
            t.Name.Value.Equals(name.Value, StringComparison.OrdinalIgnoreCase));

        return match is null
            ? Result<CashRegisterTerminal>.Failure("Cash register terminal not found.")
            : Result<CashRegisterTerminal>.Success(match);
    }

    public async Task<Result> Save(CashRegisterTerminal entity)
    {
        var result = await legacyPort.SaveCashRegisterTerminalToLegacyAsync(entity);
        return result.IsFailure ? Result.Failure(result.Error) : Result.Success();
    }

    public async Task<Result> Delete(Guid id)
    {
        var terminalResult = await GetById(id);
        if (terminalResult.IsFailure)
            return Result.Failure(terminalResult.Error);

        return await legacyPort.DeleteCashRegisterTerminalFromLegacyAsync(
            terminalResult.Value.TenantId,
            id);
    }
}
