using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Infrastructure.Legacy.Mappers.Sales;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Finance;

internal static class LegacyCommissionMapper
{
    public static Result<Commission> ToDomain(LegacyCommissionRow row)
    {
        var tenantResult = TenantId.Create(row.CompanyId);
        if (tenantResult.IsFailure)
            return Result<Commission>.Failure(tenantResult.Error);

        var amountResult = Money.Create(row.Amount);
        if (amountResult.IsFailure)
            return Result<Commission>.Failure(amountResult.Error);

        var isPaid = LegacyReceivableMapper.IsPaid(row.Paid);
        Guid? paymentPayableId = row.PaymentPayableLegacyId is > 0
            ? LegacyFinanceIds.Payable(row.PaymentPayableLegacyId.Value)
            : null;

        return Commission.Reconstitute(
            LegacyFinanceIds.Commission(row.Id),
            tenantResult.Value,
            LegacySalesIds.Sale(row.SaleLegacyId),
            row.SellerLegacyId,
            amountResult.Value,
            isPaid,
            row.PaidAt,
            paymentPayableId,
            row.CreatedAt,
            row.PaidAt ?? row.CreatedAt);
    }

    public static LegacyCommissionRow ToRow(Commission commission, int? legacyId = null)
    {
        var saleLegacyId = LegacySalesIds.ParseSaleLegacyId(commission.SaleId) ?? 0;
        var paymentPayableLegacyId = commission.PaymentPayableId is Guid payableId
            ? LegacyFinanceIds.ParsePayableLegacyId(payableId)
            : null;

        return new LegacyCommissionRow
        {
            Id = legacyId ?? LegacyFinanceIds.ParseCommissionLegacyId(commission.Id) ?? 0,
            CompanyId = commission.TenantId.Value,
            Description = "Comissão venda",
            Amount = commission.Amount.Amount,
            SellerLegacyId = commission.SellerLegacyId,
            SaleLegacyId = saleLegacyId,
            Paid = commission.IsPaid ? "Sim" : "Não",
            CreatedAt = commission.CreatedAt,
            PaidAt = commission.PaidAt,
            PaymentPayableLegacyId = paymentPayableLegacyId
        };
    }
}
