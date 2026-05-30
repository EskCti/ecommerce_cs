using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Application.Ports;

public interface IFinanceLegacyPort
{
    Task<Result<IReadOnlyList<Receivable>>> GetManualReceivablesFromLegacyAsync(
        TenantId tenantId,
        PaymentStatus? status,
        DateTime? dueFrom,
        DateTime? dueTo,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<Result<int>> CountManualReceivablesFromLegacyAsync(
        TenantId tenantId,
        PaymentStatus? status,
        DateTime? dueFrom,
        DateTime? dueTo,
        CancellationToken ct = default);

    Task<Result> SaveManualReceivableToLegacyAsync(Receivable receivable, CancellationToken ct = default);
    Task<Result> DeleteManualReceivableFromLegacyAsync(TenantId tenantId, Guid receivableId, CancellationToken ct = default);

    Task<Result<IReadOnlyList<Payable>>> GetPayablesFromLegacyAsync(
        TenantId tenantId,
        AccountType? type,
        PaymentStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<Result<int>> CountPayablesFromLegacyAsync(
        TenantId tenantId,
        AccountType? type,
        PaymentStatus? status,
        CancellationToken ct = default);

    Task<Result> SavePayableToLegacyAsync(Payable payable, CancellationToken ct = default);
    Task<Result> DeletePayableFromLegacyAsync(TenantId tenantId, Guid payableId, CancellationToken ct = default);

    Task<Result<IReadOnlyList<Commission>>> GetCommissionsFromLegacyAsync(
        TenantId tenantId,
        bool? isPaid,
        int? sellerLegacyId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<Result<int>> CountCommissionsFromLegacyAsync(
        TenantId tenantId,
        bool? isPaid,
        int? sellerLegacyId,
        CancellationToken ct = default);

    Task<Result> SaveCommissionToLegacyAsync(Commission commission, CancellationToken ct = default);
    Task<Result> DeleteCommissionBySaleFromLegacyAsync(TenantId tenantId, Guid saleId, CancellationToken ct = default);

    Task<Result<IReadOnlyList<FinanceAttachment>>> GetReceivableAttachmentsFromLegacyAsync(
        TenantId tenantId,
        Guid receivableId,
        CancellationToken ct = default);

    Task<Result<IReadOnlyList<FinanceAttachment>>> GetPayableAttachmentsFromLegacyAsync(
        TenantId tenantId,
        Guid payableId,
        CancellationToken ct = default);

    Task<Result> SaveReceivableAttachmentToLegacyAsync(Receivable receivable, FinanceAttachment attachment, CancellationToken ct = default);
    Task<Result> SavePayableAttachmentToLegacyAsync(Payable payable, FinanceAttachment attachment, CancellationToken ct = default);

    Task<Result<CashFlowSummary>> GetCashFlowFromLegacyAsync(
        TenantId tenantId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}

public sealed record CashFlowSummary(
    decimal TotalInflow,
    decimal TotalOutflow,
    decimal Balance,
    IReadOnlyList<CashFlowLine> Lines);

public sealed record CashFlowLine(
    string Type,
    string Description,
    decimal Amount,
    DateTime Date);

public interface IFinanceCancellationPort
{
    Task<Result> RemoveBySaleIdAsync(TenantId tenantId, Guid saleId, CancellationToken ct = default);
}
