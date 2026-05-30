using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Application.DTOs;

public sealed record ReceivableOutputDto
{
    public required Guid Id { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required DateTime DueDate { get; init; }
    public required PaymentStatus Status { get; init; }
    public DateTime? SettledAt { get; init; }
    public Guid? SaleId { get; init; }
    public int? PersonLegacyId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public IReadOnlyList<FinanceAttachmentOutputDto> Attachments { get; init; } = [];

    public static ReceivableOutputDto FromDomain(Receivable receivable) =>
        new()
        {
            Id = receivable.Id,
            Description = receivable.Description,
            Amount = receivable.Amount.Amount,
            Currency = receivable.Amount.Currency,
            DueDate = receivable.DueDate.Value,
            Status = receivable.Status,
            SettledAt = receivable.SettledAt,
            SaleId = receivable.SaleId,
            PersonLegacyId = receivable.PersonLegacyId,
            CreatedAt = receivable.CreatedAt,
            UpdatedAt = receivable.UpdatedAt,
            Attachments = receivable.Attachments.Select(FinanceAttachmentOutputDto.FromDomain).ToList()
        };
}

public sealed record CreateReceivableInputDto
{
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime DueDate { get; init; }
    public int? PersonLegacyId { get; init; }
}

public sealed record UpdateReceivableInputDto
{
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime DueDate { get; init; }
    public int? PersonLegacyId { get; init; }
}

public sealed record SettleAccountInputDto
{
    public required DateTime SettlementDate { get; init; }
}

public sealed record AddFinanceAttachmentInputDto
{
    public required string Name { get; init; }
    public required string Path { get; init; }
}

public sealed record FinanceAttachmentOutputDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required DateTime CreatedAt { get; init; }

    public static FinanceAttachmentOutputDto FromDomain(FinanceAttachment attachment) =>
        new()
        {
            Id = attachment.Id,
            Name = attachment.Meta.Name,
            Path = attachment.Meta.Path,
            CreatedAt = attachment.CreatedAt
        };
}

public sealed record PayableOutputDto
{
    public required Guid Id { get; init; }
    public required AccountType Type { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required DateTime DueDate { get; init; }
    public required PaymentStatus Status { get; init; }
    public DateTime? SettledAt { get; init; }
    public int? RecurrenceDays { get; init; }
    public string? RecurrenceLabel { get; init; }
    public int? PersonLegacyId { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? CommissionId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public IReadOnlyList<FinanceAttachmentOutputDto> Attachments { get; init; } = [];

    public static PayableOutputDto FromDomain(Payable payable) =>
        new()
        {
            Id = payable.Id,
            Type = payable.Type,
            Description = payable.Description,
            Amount = payable.Amount.Amount,
            Currency = payable.Amount.Currency,
            DueDate = payable.DueDate.Value,
            Status = payable.Status,
            SettledAt = payable.SettledAt,
            RecurrenceDays = payable.Recurrence?.IntervalDays,
            RecurrenceLabel = payable.Recurrence?.Label,
            PersonLegacyId = payable.PersonLegacyId,
            ProductId = payable.ProductId,
            CommissionId = payable.CommissionId,
            CreatedAt = payable.CreatedAt,
            UpdatedAt = payable.UpdatedAt,
            Attachments = payable.Attachments.Select(FinanceAttachmentOutputDto.FromDomain).ToList()
        };
}

public sealed record CreatePayableInputDto
{
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime DueDate { get; init; }
    public int? RecurrenceDays { get; init; }
    public int? PersonLegacyId { get; init; }
}

public sealed record UpdatePayableInputDto
{
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime DueDate { get; init; }
    public int? RecurrenceDays { get; init; }
    public int? PersonLegacyId { get; init; }
}

public sealed record CommissionOutputDto
{
    public required Guid Id { get; init; }
    public required Guid SaleId { get; init; }
    public required int SellerLegacyId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required bool IsPaid { get; init; }
    public DateTime? PaidAt { get; init; }
    public Guid? PaymentPayableId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static CommissionOutputDto FromDomain(Commission commission) =>
        new()
        {
            Id = commission.Id,
            SaleId = commission.SaleId,
            SellerLegacyId = commission.SellerLegacyId,
            Amount = commission.Amount.Amount,
            Currency = commission.Amount.Currency,
            IsPaid = commission.IsPaid,
            PaidAt = commission.PaidAt,
            PaymentPayableId = commission.PaymentPayableId,
            CreatedAt = commission.CreatedAt,
            UpdatedAt = commission.UpdatedAt
        };
}

public sealed record PayCommissionsBatchInputDto
{
    public required IReadOnlyList<Guid> CommissionIds { get; init; }
    public required DateTime PaymentDate { get; init; }
}

public sealed record CashFlowOutputDto
{
    public required decimal TotalInflow { get; init; }
    public required decimal TotalOutflow { get; init; }
    public required decimal Balance { get; init; }
    public required IReadOnlyList<CashFlowLineOutputDto> Lines { get; init; }
}

public sealed record CashFlowLineOutputDto
{
    public required string Type { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime Date { get; init; }
}

public sealed record PagedResultDto<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Total { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}
