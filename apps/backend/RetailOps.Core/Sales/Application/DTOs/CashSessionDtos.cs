using RetailOps.Core.Sales.Domain.Entities;

namespace RetailOps.Core.Sales.Application.DTOs;

public sealed record OpenCashSessionInputDto
{
    public required Guid TerminalId { get; init; }
    public required Guid ManagerUserId { get; init; }
    public required string ManagerPin { get; init; }
    public required decimal OpeningFloat { get; init; }
}

public sealed record RegisterCashWithdrawalInputDto
{
    public required decimal Amount { get; init; }
}

public sealed record CloseCashSessionInputDto
{
    public required Guid ManagerUserId { get; init; }
    public required string ManagerPin { get; init; }
    public required decimal CountedCash { get; init; }
}

public sealed record CashSessionOutputDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required Guid TerminalId { get; init; }
    public required Guid OperatorUserId { get; init; }
    public required string Status { get; init; }
    public required decimal OpeningFloat { get; init; }
    public required decimal TotalSold { get; init; }
    public required decimal TotalWithdrawals { get; init; }
    public decimal? CountedCash { get; init; }
    public decimal? Breakage { get; init; }
    public required DateTime OpenedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public required IReadOnlyList<CartLineOutputDto> Lines { get; init; }

    public static CashSessionOutputDto FromDomain(CashSession session) =>
        new()
        {
            Id = session.Id,
            TenantId = session.TenantId.Value,
            TerminalId = session.TerminalId,
            OperatorUserId = session.OperatorUserId,
            Status = session.Status.ToString(),
            OpeningFloat = session.OpeningFloat,
            TotalSold = session.TotalSold,
            TotalWithdrawals = session.TotalWithdrawals,
            CountedCash = session.CountedCash,
            Breakage = session.Breakage?.Value,
            OpenedAt = session.OpenedAt,
            ClosedAt = session.ClosedAt,
            Lines = session.Lines.Select(CartLineOutputDto.FromDomain).ToList(),
        };
}
