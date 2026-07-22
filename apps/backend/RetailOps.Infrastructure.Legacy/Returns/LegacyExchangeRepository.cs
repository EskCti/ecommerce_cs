using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Application.UseCases;
using RetailOps.Core.Returns.Application.Ports;
using RetailOps.Core.Returns.Domain.Entities;
using RetailOps.Core.Returns.Domain.Repositories;
using RetailOps.Infrastructure.Legacy.Mappers.Returns;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Returns;

public sealed class LegacyExchangeRepository(LegacySasDbContext db) : IExchangeRepository, IReturnsLegacyPort
{
    public async Task<Result<Exchange>> GetById(TenantId tenantId, Guid id, CancellationToken ct = default)
    {
        var legacyId = LegacyReturnsIds.ParseExchangeLegacyId(id);
        if (legacyId is null)
            return Result<Exchange>.Failure("Invalid exchange id.");

        var row = await db.Exchanges.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == legacyId && e.CompanyId == tenantId.Value, ct);

        if (row is null)
            return Result<Exchange>.Failure("Exchange not found.");

        var (gradeIn, gradeOut) = await LoadGradeDetailsAsync(tenantId.Value, legacyId.Value, ct);
        return LegacyExchangeMapper.ToDomain(row, gradeIn, gradeOut);
    }

    public async Task<Result<IReadOnlyList<Exchange>>> ListByTenant(TenantId tenantId, CancellationToken ct = default)
    {
        var rows = await db.Exchanges.AsNoTracking()
            .Where(e => e.CompanyId == tenantId.Value)
            .OrderByDescending(e => e.ExchangeDate)
            .ToListAsync(ct);

        var exchanges = new List<Exchange>();
        foreach (var row in rows)
        {
            var mapped = LegacyExchangeMapper.ToDomain(row);
            if (mapped.IsFailure)
                return Result<IReadOnlyList<Exchange>>.Failure(mapped.Error);

            exchanges.Add(mapped.Value);
        }

        return Result<IReadOnlyList<Exchange>>.Success(exchanges);
    }

    public async Task<Result> Save(Exchange exchange, CancellationToken ct = default)
    {
        var saveLegacy = await SaveExchangeToLegacyAsync(exchange, ct);
        return saveLegacy.IsFailure ? Result.Failure(saveLegacy.Error) : Result.Success();
    }

    public Task<Result> Delete(TenantId tenantId, Guid id, CancellationToken ct = default) =>
        DeleteExchangeFromLegacyAsync(tenantId, id, ct);

    public async Task<Result<int>> SaveExchangeToLegacyAsync(Exchange exchange, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var legacyId = LegacyReturnsIds.ParseExchangeLegacyId(exchange.Id);
            var mapped = LegacyExchangeMapper.ToLegacy(exchange, legacyId);
            if (mapped.IsFailure)
                return Result<int>.Failure(mapped.Error);

            var row = mapped.Value;
            if (row.Id == 0)
            {
                db.Exchanges.Add(row);
                await db.SaveChangesAsync(ct);
                exchange.SyncIdentity(LegacyReturnsIds.Exchange(row.Id));
            }
            else
            {
                var existing = await db.Exchanges.FirstOrDefaultAsync(e => e.Id == row.Id, ct);
                if (existing is null)
                    return Result<int>.Failure("Exchange not found.");

                existing.CustomerLegacyId = row.CustomerLegacyId;
                existing.ProductInLegacyId = row.ProductInLegacyId;
                existing.ProductOutLegacyId = row.ProductOutLegacyId;
                existing.UserLegacyId = row.UserLegacyId;
                existing.ExchangeDate = row.ExchangeDate;
            }

            await db.SaveChangesAsync(ct);
            await ReplaceGradeDetailsAsync(exchange.TenantId, row.Id, exchange, ct);
            await transaction.CommitAsync(ct);
            return Result<int>.Success(row.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return Result<int>.Failure($"Failed to save exchange: {ex.Message}");
        }
    }

    public async Task<Result> DeleteExchangeFromLegacyAsync(
        TenantId tenantId,
        Guid exchangeId,
        CancellationToken ct = default)
    {
        var legacyId = LegacyReturnsIds.ParseExchangeLegacyId(exchangeId);
        if (legacyId is null)
            return Result.Failure("Invalid exchange id.");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var details = await db.GradeMovementDetails
                .Where(d => d.MovementLegacyId == legacyId
                    && d.CompanyId == tenantId.Value
                    && (d.MovementType == LegacyExchangeMovementTypes.ExchangeIn
                        || d.MovementType == LegacyExchangeMovementTypes.ExchangeOut))
                .ToListAsync(ct);

            db.GradeMovementDetails.RemoveRange(details);

            var row = await db.Exchanges
                .FirstOrDefaultAsync(e => e.Id == legacyId && e.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result.Failure("Exchange not found.");

            db.Exchanges.Remove(row);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return Result.Failure($"Failed to delete exchange: {ex.Message}");
        }
    }

    private async Task<(LegacyGradeMovementDetailRow?, LegacyGradeMovementDetailRow?)> LoadGradeDetailsAsync(
        int tenantId,
        int exchangeLegacyId,
        CancellationToken ct)
    {
        var details = await db.GradeMovementDetails.AsNoTracking()
            .Where(d => d.MovementLegacyId == exchangeLegacyId && d.CompanyId == tenantId)
            .ToListAsync(ct);

        var gradeIn = details.FirstOrDefault(d => d.MovementType == LegacyExchangeMovementTypes.ExchangeIn);
        var gradeOut = details.FirstOrDefault(d => d.MovementType == LegacyExchangeMovementTypes.ExchangeOut);
        return (gradeIn, gradeOut);
    }

    private async Task ReplaceGradeDetailsAsync(
        TenantId tenantId,
        int exchangeLegacyId,
        Exchange exchange,
        CancellationToken ct)
    {
        var existing = await db.GradeMovementDetails
            .Where(d => d.MovementLegacyId == exchangeLegacyId
                && d.CompanyId == tenantId.Value
                && (d.MovementType == LegacyExchangeMovementTypes.ExchangeIn
                    || d.MovementType == LegacyExchangeMovementTypes.ExchangeOut))
            .ToListAsync(ct);

        if (existing.Count > 0)
            db.GradeMovementDetails.RemoveRange(existing);

        if (exchange.GradeIn?.HasGrade == true)
        {
            var detailIn = LegacyExchangeMapper.ToGradeDetailLegacy(
                tenantId,
                LegacyExchangeMovementTypes.ExchangeIn,
                exchangeLegacyId,
                exchange.GradeIn);
            if (detailIn.IsSuccess)
                db.GradeMovementDetails.Add(detailIn.Value);
        }

        if (exchange.GradeOut?.HasGrade == true)
        {
            var detailOut = LegacyExchangeMapper.ToGradeDetailLegacy(
                tenantId,
                LegacyExchangeMovementTypes.ExchangeOut,
                exchangeLegacyId,
                exchange.GradeOut);
            if (detailOut.IsSuccess)
                db.GradeMovementDetails.Add(detailOut.Value);
        }

        await db.SaveChangesAsync(ct);
    }
}

public sealed class CustomerProvisioningAdapter(FindOrCreateByCpfUseCase findOrCreateByCpfUseCase)
    : ICustomerProvisioningPort
{
    public async Task<Result<Guid>> FindOrCreateByCpfAsync(
        TenantId tenantId,
        string cpf,
        string name,
        CancellationToken ct = default)
    {
        var result = await findOrCreateByCpfUseCase.Execute(
            (tenantId.Value, new FindOrCreateByCpfInputDto
            {
                Cpf = cpf,
                Name = name
            }),
            ct);

        return result.IsFailure
            ? Result<Guid>.Failure(result.Error)
            : Result<Guid>.Success(result.Value.Customer.Id);
    }
}

public sealed class ReturnsQueries(IExchangeRepository exchangeRepository)
    : Core.Returns.Application.Queries.IListExchangesQuery
{
    public async Task<Result<Core.Returns.Application.DTOs.ExchangeListPageDto>> ListByTenantAsync(
        TenantId tenantId,
        Core.Returns.Application.DTOs.ExchangeListFilter filter,
        CancellationToken ct = default)
    {
        var listResult = await exchangeRepository.ListByTenant(tenantId, ct);
        if (listResult.IsFailure)
            return Result<Core.Returns.Application.DTOs.ExchangeListPageDto>.Failure(listResult.Error);

        var query = listResult.Value.AsEnumerable();

        if (filter.From.HasValue)
            query = query.Where(e => e.RegisteredAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(e => e.RegisteredAt <= filter.To.Value);

        if (filter.CustomerId.HasValue)
            query = query.Where(e => e.CustomerId.Value == filter.CustomerId.Value);

        if (filter.ProductId.HasValue)
            query = query.Where(e =>
                e.ProductInId == filter.ProductId.Value || e.ProductOutId == filter.ProductId.Value);

        var ordered = query.OrderByDescending(e => e.RegisteredAt).ToList();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var items = ordered
            .Skip(skip)
            .Take(pageSize)
            .Select(e => new Core.Returns.Application.DTOs.ExchangeListItemDto(
                e.Id,
                e.CustomerId.Value,
                e.ProductInId,
                e.ProductOutId,
                e.RegisteredAt))
            .ToList();

        return Result<Core.Returns.Application.DTOs.ExchangeListPageDto>.Success(
            new Core.Returns.Application.DTOs.ExchangeListPageDto(items, page, pageSize, ordered.Count));
    }
}
