using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Queries;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Sales;

public sealed class SalesQueries(ISaleRepository saleRepository) : IListSalesQuery
{
    public async Task<Result<SaleListPageDto>> ListByTenantAsync(
        TenantId tenantId,
        SaleListFilter filter,
        CancellationToken ct = default)
    {
        var listResult = await saleRepository.List(tenantId, ct);
        if (listResult.IsFailure)
            return Result<SaleListPageDto>.Failure(listResult.Error);

        var query = listResult.Value.AsEnumerable();

        if (filter.From.HasValue)
            query = query.Where(s => s.CompletedAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(s => s.CompletedAt <= filter.To.Value);

        if (filter.OperatorUserId.HasValue)
            query = query.Where(s => s.OperatorUserId == filter.OperatorUserId.Value);

        var ordered = query.OrderByDescending(s => s.CompletedAt).ToList();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var items = ordered
            .Skip(skip)
            .Take(pageSize)
            .Select(s => new SaleListItemDto(
                s.Id,
                s.Total,
                s.PaymentTerms.ToString(),
                s.IsCancelled,
                s.CompletedAt))
            .ToList();

        return Result<SaleListPageDto>.Success(new SaleListPageDto(items, page, pageSize, ordered.Count));
    }
}
