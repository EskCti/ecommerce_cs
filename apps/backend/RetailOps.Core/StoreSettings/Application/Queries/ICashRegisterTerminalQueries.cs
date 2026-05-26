using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.Queries;

public interface ICashRegisterTerminalQueries
{
    Task<Result<IReadOnlyList<CashRegisterTerminalListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        CashRegisterTerminalListFilter? filter = null,
        CancellationToken ct = default);
    
    Task<Result<CashRegisterTerminalListItemDto?>> GetByIdAsync(
        TenantId tenantId,
        Guid terminalId,
        CancellationToken ct = default);
}

public sealed record CashRegisterTerminalListFilter(
    string? Status,
    string? SearchTerm,
    Guid? AssignedOperatorId,
    int Page = 1,
    int PageSize = 20);