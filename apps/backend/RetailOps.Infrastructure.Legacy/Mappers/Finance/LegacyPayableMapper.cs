using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.Catalog;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Finance;

internal static class LegacyPayableMapper
{
    public const string AttachmentType = "Pagar";

    internal static string ToLegacyType(AccountType type) => type switch
    {
        AccountType.Expense => "Conta",
        AccountType.Purchase => "Compra",
        AccountType.CommissionPayment => "Pagamento",
        _ => "Conta"
    };

    internal static AccountType FromLegacyType(string? legacyType) => legacyType switch
    {
        null => AccountType.Expense,
        var t when t.Equals("Compra", StringComparison.OrdinalIgnoreCase) => AccountType.Purchase,
        var t when t.Equals("Pagamento", StringComparison.OrdinalIgnoreCase) => AccountType.CommissionPayment,
        _ => AccountType.Expense
    };

    public static Result<Payable> ToDomain(
        LegacyPayableRow row,
        IEnumerable<FinanceAttachment>? attachments = null)
    {
        var tenantResult = TenantId.Create(row.CompanyId);
        if (tenantResult.IsFailure)
            return Result<Payable>.Failure(tenantResult.Error);

        var amountResult = Money.Create(row.Amount);
        if (amountResult.IsFailure)
            return Result<Payable>.Failure(amountResult.Error);

        var dueDateResult = DueDate.Create(row.DueDate);
        if (dueDateResult.IsFailure)
            return Result<Payable>.Failure(dueDateResult.Error);

        Recurrence? recurrence = null;
        if (row.FrequencyDays is not null)
        {
            var recurrenceResult = Recurrence.Create(row.FrequencyDays.Value);
            if (recurrenceResult.IsFailure)
                return Result<Payable>.Failure(recurrenceResult.Error);
            recurrence = recurrenceResult.Value;
        }

        var status = LegacyReceivableMapper.IsPaid(row.Paid) ? PaymentStatus.Settled : PaymentStatus.Open;
        var type = FromLegacyType(row.Type);

        Guid? productId = null;
        if (type == AccountType.Purchase && row.ReferenceId is > 0)
            productId = LegacyCatalogIds.Product(row.ReferenceId.Value);

        Guid? commissionId = null;
        if (type == AccountType.CommissionPayment && row.ReferenceId is > 0)
            commissionId = LegacyFinanceIds.Commission(row.ReferenceId.Value);

        return Payable.Reconstitute(
            LegacyFinanceIds.Payable(row.Id),
            tenantResult.Value,
            type,
            row.Description,
            amountResult.Value,
            dueDateResult.Value,
            status,
            row.SettledAt,
            recurrence,
            row.PersonId > 0 ? row.PersonId : null,
            productId,
            commissionId,
            row.CreatedAt,
            row.SettledAt ?? row.CreatedAt,
            attachments);
    }

    public static LegacyPayableRow ToRow(Payable payable, int? legacyId = null)
    {
        int? referenceId = payable.Type switch
        {
            AccountType.Purchase => payable.ProductId is Guid productId
                ? LegacyCatalogIds.ParseLegacyId(productId, "0009")
                : null,
            AccountType.CommissionPayment => payable.CommissionId is Guid commissionId
                ? LegacyFinanceIds.ParseCommissionLegacyId(commissionId)
                : null,
            _ => null
        };

        return new LegacyPayableRow
        {
            Id = legacyId ?? LegacyFinanceIds.ParsePayableLegacyId(payable.Id) ?? 0,
            CompanyId = payable.TenantId.Value,
            Type = ToLegacyType(payable.Type),
            Description = payable.Description,
            PersonId = payable.PersonLegacyId ?? 0,
            Amount = payable.Amount.Amount,
            DueDate = payable.DueDate.Value,
            Paid = payable.Status == PaymentStatus.Settled ? "Sim" : "Não",
            FrequencyDays = payable.Recurrence?.IntervalDays,
            ReferenceId = referenceId,
            SettledAt = payable.SettledAt,
            CreatedAt = payable.CreatedAt
        };
    }
}
