using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Infrastructure.Legacy.Mappers.Sales;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Sales;

public sealed class LegacyCashSessionRepository(LegacySasDbContext db) : ICashSessionRepository
{
    public async Task<Result<CashSession>> GetById(Guid id)
    {
        var legacyId = LegacySalesIds.ParseCashSessionLegacyId(id);
        if (legacyId is null)
            return Result<CashSession>.Failure("Invalid cash session id.");

        return await LoadSessionAsync(legacyId.Value);
    }

    public async Task<Result<CashSession?>> GetOpenByOperator(TenantId tenantId, Guid operatorUserId)
    {
        var operatorLegacyId = LegacySalesIds.ParseOperatorLegacyId(operatorUserId);
        if (operatorLegacyId is null)
            return Result<CashSession?>.Failure("Invalid operator id.");

        var row = await db.CashSessions.AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.CompanyId == tenantId.Value
                && r.OperatorLegacyUserId == operatorLegacyId
                && r.Status == "Aberto");

        if (row is null)
            return Result<CashSession?>.Success(null);

        var mapped = await LoadSessionAsync(row.Id);
        return mapped.IsFailure
            ? Result<CashSession?>.Failure(mapped.Error)
            : Result<CashSession?>.Success(mapped.Value);
    }

    public async Task<Result<CashSession?>> GetOpenByTerminal(TenantId tenantId, Guid terminalId)
    {
        var terminalLegacyId = LegacySalesIds.ParseTerminalLegacyId(terminalId);
        if (terminalLegacyId is null)
            return Result<CashSession?>.Failure("Invalid terminal id.");

        var row = await db.CashSessions.AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.CompanyId == tenantId.Value
                && r.TerminalLegacyId == terminalLegacyId
                && r.Status == "Aberto");

        if (row is null)
            return Result<CashSession?>.Success(null);

        var mapped = await LoadSessionAsync(row.Id);
        return mapped.IsFailure
            ? Result<CashSession?>.Failure(mapped.Error)
            : Result<CashSession?>.Success(mapped.Value);
    }

    public async Task<Result> Save(CashSession entity)
    {
        try
        {
            var legacyId = LegacySalesIds.ParseCashSessionLegacyId(entity.Id);
            var row = LegacyCashSessionMapper.ToRow(entity, legacyId);

            if (legacyId is null)
            {
                db.CashSessions.Add(row);
                await db.SaveChangesAsync();
                entity.SyncIdentity(LegacySalesIds.CashSession(row.Id));
                legacyId = row.Id;
            }
            else
            {
                var tracked = await db.CashSessions.FirstOrDefaultAsync(r => r.Id == legacyId);
                if (tracked is null)
                    return Result.Failure("Cash session not found.");

                tracked.Status = row.Status;
                tracked.OpeningFloat = row.OpeningFloat;
                tracked.TotalSold = row.TotalSold;
                tracked.CountedCash = row.CountedCash;
                tracked.Breakage = row.Breakage;
                tracked.ClosedAt = row.ClosedAt;
            }

            await SyncCartLinesAsync(entity, legacyId.Value);
            await SyncWithdrawalsAsync(entity, legacyId.Value);
            await db.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to save cash session: {ex.Message}");
        }
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Cash session cannot be deleted."));

    private async Task<Result<CashSession>> LoadSessionAsync(int legacyId)
    {
        var row = await db.CashSessions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == legacyId);
        if (row is null)
            return Result<CashSession>.Failure("Cash session not found.");

        var lineRows = await db.CartItems.AsNoTracking()
            .Where(i => i.CashSessionLegacyId == legacyId && i.SaleLegacyId == 0)
            .ToListAsync();

        var lines = new List<SaleLine>();
        foreach (var lineRow in lineRows)
        {
            var mapped = LegacyCartItemMapper.ToDomain(lineRow);
            if (mapped.IsSuccess)
                lines.Add(mapped.Value);
        }

        var withdrawalRows = await db.Withdrawals.AsNoTracking()
            .Where(w => w.CashSessionLegacyId == legacyId)
            .ToListAsync();

        var withdrawals = withdrawalRows.Select(w =>
            CashWithdrawal.Reconstitute(
                LegacySalesIds.Withdrawal(w.Id),
                w.Amount,
                w.RegisteredAt).Value).ToList();

        var sessionResult = LegacyCashSessionMapper.ToDomain(row, lines, withdrawals);
        return sessionResult.IsFailure
            ? Result<CashSession>.Failure(sessionResult.Error)
            : Result<CashSession>.Success(sessionResult.Value);
    }

    private async Task SyncCartLinesAsync(CashSession session, int cashSessionLegacyId)
    {
        var existing = await db.CartItems
            .Where(i => i.CashSessionLegacyId == cashSessionLegacyId && i.SaleLegacyId == 0)
            .ToListAsync();

        db.CartItems.RemoveRange(existing);

        foreach (var line in session.Lines)
        {
            var lineLegacyId = LegacySalesIds.ParseCartLineLegacyId(line.Id);
            db.CartItems.Add(LegacyCartItemMapper.ToRow(line, session, cashSessionLegacyId, lineLegacyId));
        }
    }

    private async Task SyncWithdrawalsAsync(CashSession session, int cashSessionLegacyId)
    {
        var existing = await db.Withdrawals
            .Where(w => w.CashSessionLegacyId == cashSessionLegacyId)
            .ToListAsync();

        db.Withdrawals.RemoveRange(existing);

        foreach (var withdrawal in session.Withdrawals)
        {
            var legacyId = LegacySalesIds.ParseWithdrawalLegacyId(withdrawal.Id);
            db.Withdrawals.Add(new Persistence.Entities.LegacyWithdrawalRow
            {
                Id = legacyId ?? 0,
                CompanyId = session.TenantId.Value,
                CashSessionLegacyId = cashSessionLegacyId,
                Amount = withdrawal.Amount,
                RegisteredAt = withdrawal.RegisteredAt,
            });
        }
    }
}
