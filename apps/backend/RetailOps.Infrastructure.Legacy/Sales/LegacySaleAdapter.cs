using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Infrastructure.Legacy.Mappers.Sales;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Infrastructure.Legacy.Sales;

public sealed class LegacySaleAdapter(LegacySasDbContext db) : ISalesLegacyPort
{
    public async Task<Result<int>> OpenSession(CashSession session, CancellationToken ct = default)
    {
        try
        {
            var row = LegacyCashSessionMapper.ToRow(session);
            db.CashSessions.Add(row);
            await db.SaveChangesAsync(ct);
            session.SyncIdentity(LegacySalesIds.CashSession(row.Id));
            return Result<int>.Success(row.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to open legacy cash session: {ex.Message}");
        }
    }

    public Task<Result> SaveSession(CashSession session, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public async Task<Result<int>> AddCartLine(CashSession session, SaleLine line, CancellationToken ct = default)
    {
        try
        {
            var sessionLegacyId = LegacySalesIds.ParseCashSessionLegacyId(session.Id);
            if (sessionLegacyId is null)
                return Result<int>.Failure("Cash session legacy id not found.");

            var row = LegacyCartItemMapper.ToRow(line, session, sessionLegacyId.Value);
            db.CartItems.Add(row);
            await db.SaveChangesAsync(ct);
            line.SyncIdentity(LegacySalesIds.CartLine(row.Id));
            return Result<int>.Success(row.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to add cart line: {ex.Message}");
        }
    }

    public async Task<Result> RemoveCartLine(CashSession session, Guid lineId, CancellationToken ct = default)
    {
        try
        {
            var lineLegacyId = LegacySalesIds.ParseCartLineLegacyId(lineId);
            if (lineLegacyId is null)
                return Result.Success();

            var row = await db.CartItems.FirstOrDefaultAsync(i => i.Id == lineLegacyId, ct);
            if (row is not null)
            {
                db.CartItems.Remove(row);
                await db.SaveChangesAsync(ct);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to remove cart line: {ex.Message}");
        }
    }

    public async Task<Result<int>> FinalizeSale(CashSession session, Sale sale, CancellationToken ct = default)
    {
        try
        {
            var sessionLegacyId = LegacySalesIds.ParseCashSessionLegacyId(session.Id);
            if (sessionLegacyId is null)
                return Result<int>.Failure("Cash session legacy id not found.");

            var row = LegacySaleMapper.ToRow(sale, sessionLegacyId.Value);
            db.Receivables.Add(row);
            await db.SaveChangesAsync(ct);
            sale.SyncIdentity(LegacySalesIds.Sale(row.Id));

            var pending = await db.CartItems
                .Where(i => i.CashSessionLegacyId == sessionLegacyId && i.SaleLegacyId == 0)
                .ToListAsync(ct);

            foreach (var item in pending)
                item.SaleLegacyId = row.Id;

            await db.SaveChangesAsync(ct);
            return Result<int>.Success(row.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to finalize sale in legacy: {ex.Message}");
        }
    }

    public async Task<Result> CloseSession(CashSession session, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacySalesIds.ParseCashSessionLegacyId(session.Id);
            if (legacyId is null)
                return Result.Failure("Cash session legacy id not found.");

            var row = await db.CashSessions.FirstOrDefaultAsync(c => c.Id == legacyId, ct);
            if (row is null)
                return Result.Failure("Cash session not found in legacy.");

            var mapped = LegacyCashSessionMapper.ToRow(session, legacyId);
            row.Status = mapped.Status;
            row.CountedCash = mapped.CountedCash;
            row.Breakage = mapped.Breakage;
            row.ClosedAt = mapped.ClosedAt;
            row.TotalSold = mapped.TotalSold;

            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to close legacy cash session: {ex.Message}");
        }
    }

    public async Task<Result> CancelSale(Sale sale, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacySalesIds.ParseSaleLegacyId(sale.Id);
            if (legacyId is null)
                return Result.Failure("Sale legacy id not found.");

            var row = await db.Receivables.FirstOrDefaultAsync(r => r.Id == legacyId, ct);
            if (row is null)
                return Result.Failure("Sale not found in legacy.");

            row.Cancelled = "Sim";
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to cancel sale in legacy: {ex.Message}");
        }
    }
}
