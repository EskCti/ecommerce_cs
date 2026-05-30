using RetailOps.Core.Finance.Application.DTOs;
using RetailOps.Core.Finance.Application.Ports;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Application.Queries;

public sealed record ReceivableListFilter(
    PaymentStatus? Status,
    DateTime? DueFrom,
    DateTime? DueTo,
    int Page = 1,
    int PageSize = 20);

public sealed record PayableListFilter(
    AccountType? Type,
    PaymentStatus? Status,
    int Page = 1,
    int PageSize = 20);

public sealed record CommissionListFilter(
    bool? IsPaid,
    int? SellerLegacyId,
    int Page = 1,
    int PageSize = 20);

public interface IFinanceQueries
{
    Task<Result<PagedResultDto<ReceivableOutputDto>>> ListReceivablesAsync(
        TenantId tenantId,
        ReceivableListFilter filter,
        CancellationToken ct = default);

    Task<Result<PagedResultDto<PayableOutputDto>>> ListPayablesAsync(
        TenantId tenantId,
        PayableListFilter filter,
        CancellationToken ct = default);

    Task<Result<PagedResultDto<PayableOutputDto>>> ListPurchasesAsync(
        TenantId tenantId,
        PayableListFilter filter,
        CancellationToken ct = default);

    Task<Result<PagedResultDto<CommissionOutputDto>>> ListCommissionsAsync(
        TenantId tenantId,
        CommissionListFilter filter,
        CancellationToken ct = default);

    Task<Result<CashFlowOutputDto>> GetCashFlowAsync(
        TenantId tenantId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}

public sealed class CashFlowAggregator(IFinanceLegacyPort legacyPort)
{
    public async Task<Result<CashFlowOutputDto>> AggregateAsync(
        TenantId tenantId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        if (from == default || to == default)
            return Result<CashFlowOutputDto>.Failure("Date range is required.");

        if (from.Date > to.Date)
            return Result<CashFlowOutputDto>.Failure("Start date must be before end date.");

        var summaryResult = await legacyPort.GetCashFlowFromLegacyAsync(tenantId, from, to, ct);
        if (summaryResult.IsFailure)
            return Result<CashFlowOutputDto>.Failure(summaryResult.Error);

        var summary = summaryResult.Value;
        return Result<CashFlowOutputDto>.Success(new CashFlowOutputDto
        {
            TotalInflow = summary.TotalInflow,
            TotalOutflow = summary.TotalOutflow,
            Balance = summary.Balance,
            Lines = summary.Lines
                .Select(line => new CashFlowLineOutputDto
                {
                    Type = line.Type,
                    Description = line.Description,
                    Amount = line.Amount,
                    Date = line.Date
                })
                .ToList()
        });
    }
}
