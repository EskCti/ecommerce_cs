using RetailOps.Core.Finance.Application.DTOs;
using RetailOps.Core.Finance.Application.Ports;
using RetailOps.Core.Finance.Application.Queries;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Finance;

public sealed class FinanceQueries(FinanceLegacyAdapter legacyPort, CashFlowAggregator cashFlowAggregator)
    : IFinanceQueries
{
    public async Task<Result<PagedResultDto<ReceivableOutputDto>>> ListReceivablesAsync(
        TenantId tenantId,
        ReceivableListFilter filter,
        CancellationToken ct = default)
    {
        var itemsResult = await legacyPort.GetManualReceivablesFromLegacyAsync(
            tenantId,
            filter.Status,
            filter.DueFrom,
            filter.DueTo,
            filter.Page,
            filter.PageSize,
            ct);

        if (itemsResult.IsFailure)
            return Result<PagedResultDto<ReceivableOutputDto>>.Failure(itemsResult.Error);

        var totalResult = await legacyPort.CountManualReceivablesFromLegacyAsync(
            tenantId,
            filter.Status,
            filter.DueFrom,
            filter.DueTo,
            ct);

        if (totalResult.IsFailure)
            return Result<PagedResultDto<ReceivableOutputDto>>.Failure(totalResult.Error);

        return Result<PagedResultDto<ReceivableOutputDto>>.Success(new PagedResultDto<ReceivableOutputDto>
        {
            Items = itemsResult.Value.Select(ReceivableOutputDto.FromDomain).ToList(),
            Total = totalResult.Value,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public async Task<Result<PagedResultDto<PayableOutputDto>>> ListPayablesAsync(
        TenantId tenantId,
        PayableListFilter filter,
        CancellationToken ct = default)
    {
        var itemsResult = await legacyPort.GetPayablesFromLegacyAsync(
            tenantId,
            filter.Type,
            filter.Status,
            filter.Page,
            filter.PageSize,
            ct);

        if (itemsResult.IsFailure)
            return Result<PagedResultDto<PayableOutputDto>>.Failure(itemsResult.Error);

        var totalResult = await legacyPort.CountPayablesFromLegacyAsync(
            tenantId,
            filter.Type,
            filter.Status,
            ct);

        if (totalResult.IsFailure)
            return Result<PagedResultDto<PayableOutputDto>>.Failure(totalResult.Error);

        return Result<PagedResultDto<PayableOutputDto>>.Success(new PagedResultDto<PayableOutputDto>
        {
            Items = itemsResult.Value.Select(PayableOutputDto.FromDomain).ToList(),
            Total = totalResult.Value,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public Task<Result<PagedResultDto<PayableOutputDto>>> ListPurchasesAsync(
        TenantId tenantId,
        PayableListFilter filter,
        CancellationToken ct = default)
        => ListPayablesAsync(tenantId, filter with { Type = AccountType.Purchase }, ct);

    public async Task<Result<PagedResultDto<CommissionOutputDto>>> ListCommissionsAsync(
        TenantId tenantId,
        CommissionListFilter filter,
        CancellationToken ct = default)
    {
        var itemsResult = await legacyPort.GetCommissionsFromLegacyAsync(
            tenantId,
            filter.IsPaid,
            filter.SellerLegacyId,
            filter.Page,
            filter.PageSize,
            ct);

        if (itemsResult.IsFailure)
            return Result<PagedResultDto<CommissionOutputDto>>.Failure(itemsResult.Error);

        var totalResult = await legacyPort.CountCommissionsFromLegacyAsync(
            tenantId,
            filter.IsPaid,
            filter.SellerLegacyId,
            ct);

        if (totalResult.IsFailure)
            return Result<PagedResultDto<CommissionOutputDto>>.Failure(totalResult.Error);

        return Result<PagedResultDto<CommissionOutputDto>>.Success(new PagedResultDto<CommissionOutputDto>
        {
            Items = itemsResult.Value.Select(CommissionOutputDto.FromDomain).ToList(),
            Total = totalResult.Value,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public Task<Result<CashFlowOutputDto>> GetCashFlowAsync(
        TenantId tenantId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
        => cashFlowAggregator.AggregateAsync(tenantId, from, to, ct);
}
