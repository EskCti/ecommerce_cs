using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Infrastructure.Legacy.Mappers.Sales;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Sales;

public sealed class LegacySaleRepository(LegacySasDbContext db) : ISaleRepository
{
    public async Task<Result<Sale>> GetById(Guid id)
    {
        var legacyId = LegacySalesIds.ParseSaleLegacyId(id);
        if (legacyId is null)
            return Result<Sale>.Failure("Invalid sale id.");

        var row = await db.Receivables.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == legacyId && r.Type.Contains("Venda"));

        if (row is null)
            return Result<Sale>.Failure("Sale not found.");

        var lines = await LoadSaleLinesAsync(legacyId.Value);
        var mapped = LegacySaleMapper.ToDomain(row, lines);
        return mapped.IsFailure
            ? Result<Sale>.Failure(mapped.Error)
            : Result<Sale>.Success(mapped.Value);
    }

    public async Task<Result<IReadOnlyList<Sale>>> List(TenantId tenantId, CancellationToken ct = default)
    {
        var rows = await db.Receivables.AsNoTracking()
            .Where(r => r.CompanyId == tenantId.Value && r.Type.Contains("Venda"))
            .OrderByDescending(r => r.CompletedAt ?? r.DueDate)
            .ToListAsync(ct);

        var sales = new List<Sale>();
        foreach (var row in rows)
        {
            var lines = await LoadSaleLinesAsync(row.Id, ct);
            var mapped = LegacySaleMapper.ToDomain(row, lines);
            if (mapped.IsSuccess)
                sales.Add(mapped.Value);
        }

        return Result<IReadOnlyList<Sale>>.Success(sales);
    }

    public async Task<Result> Save(Sale entity)
    {
        try
        {
            var cashSessionLegacyId = LegacySalesIds.ParseCashSessionLegacyId(entity.CashSessionId);
            if (cashSessionLegacyId is null)
                return Result.Failure("Invalid cash session id for sale.");

            var legacyId = LegacySalesIds.ParseSaleLegacyId(entity.Id);
            var row = LegacySaleMapper.ToRow(entity, cashSessionLegacyId.Value, legacyId);

            if (legacyId is null)
            {
                db.Receivables.Add(row);
                await db.SaveChangesAsync();
                entity.SyncIdentity(LegacySalesIds.Sale(row.Id));
                legacyId = row.Id;
            }
            else
            {
                var tracked = await db.Receivables.FirstOrDefaultAsync(r => r.Id == legacyId);
                if (tracked is null)
                    return Result.Failure("Sale not found.");

                tracked.Amount = row.Amount;
                tracked.Paid = row.Paid;
                tracked.Cancelled = row.Cancelled;
                tracked.Discount = row.Discount;
                tracked.ChangeAmount = row.ChangeAmount;
                tracked.CommissionAmount = row.CommissionAmount;
                tracked.CompletedAt = row.CompletedAt;
            }

            await MarkCartItemsAsSoldAsync(entity, cashSessionLegacyId.Value, legacyId.Value);
            await db.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to save sale: {ex.Message}");
        }
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Sale cannot be deleted. Use cancellation."));

    private async Task<List<SaleLine>> LoadSaleLinesAsync(int saleLegacyId, CancellationToken ct = default)
    {
        var lineRows = await db.CartItems.AsNoTracking()
            .Where(i => i.SaleLegacyId == saleLegacyId)
            .ToListAsync(ct);

        var lines = new List<SaleLine>();
        foreach (var lineRow in lineRows)
        {
            var mapped = LegacyCartItemMapper.ToDomain(lineRow);
            if (mapped.IsSuccess)
                lines.Add(mapped.Value);
        }

        return lines;
    }

    private async Task MarkCartItemsAsSoldAsync(Sale sale, int cashSessionLegacyId, int saleLegacyId)
    {
        var pending = await db.CartItems
            .Where(i => i.CashSessionLegacyId == cashSessionLegacyId && i.SaleLegacyId == 0)
            .ToListAsync();

        foreach (var row in pending)
            row.SaleLegacyId = saleLegacyId;

        if (pending.Count == 0)
        {
            foreach (var line in sale.Lines)
            {
                var productLegacyId = LegacySalesIds.ParseProductLegacyId(line.ProductId) ?? 0;
                db.CartItems.Add(new Persistence.Entities.LegacyCartItemRow
                {
                    CompanyId = sale.TenantId.Value,
                    CashSessionLegacyId = cashSessionLegacyId,
                    ProductLegacyId = productLegacyId,
                    Barcode = line.Barcode,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice.Value,
                    SaleLegacyId = saleLegacyId,
                    GradeOptionIds = string.Join(',', line.GradeOptionIds),
                    Status = "Pronto",
                });
            }
        }
    }
}
