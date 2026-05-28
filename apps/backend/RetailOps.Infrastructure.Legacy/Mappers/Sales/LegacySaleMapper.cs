using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Sales;

internal static class LegacySaleMapper
{
    public static Result<Sale> ToDomain(LegacyReceivableRow row, IEnumerable<SaleLine> lines)
    {
        var tenantIdResult = TenantId.Create(row.CompanyId);
        if (tenantIdResult.IsFailure)
            return Result<Sale>.Failure(tenantIdResult.Error);

        if (row.CashSessionLegacyId is null or <= 0)
            return Result<Sale>.Failure("Sale cash session reference is required.");

        var discountResult = Discount.Create(row.Discount ?? 0);
        if (discountResult.IsFailure)
            return Result<Sale>.Failure(discountResult.Error);

        var changeResult = ChangeAmount.Create(row.ChangeAmount ?? 0);
        if (changeResult.IsFailure)
            return Result<Sale>.Failure(changeResult.Error);

        var paymentTerms = row.Type.Contains("Fiado", StringComparison.OrdinalIgnoreCase)
            ? PaymentTerms.Credit
            : PaymentTerms.Cash;

        var cashSessionId = LegacySalesIds.CashSession(row.CashSessionLegacyId.Value);
        var operatorId = row.OperatorLegacyUserId is > 0
            ? LegacyOperatorUserId(row.OperatorLegacyUserId.Value)
            : Guid.Empty;

        var paymentMethodId = row.PaymentMethodLegacyId is > 0
            ? Mappers.StoreSettings.LegacyStoreSettingsIds.PaymentMethod(row.PaymentMethodLegacyId.Value)
            : Guid.Empty;

        Guid? customerId = row.CustomerLegacyId is > 0
            ? Mappers.Crm.LegacyCrmIds.Customer(row.CustomerLegacyId.Value)
            : null;

        var isCancelled = row.Cancelled is not null
            && (row.Cancelled.Equals("S", StringComparison.OrdinalIgnoreCase)
                || row.Cancelled.Equals("Sim", StringComparison.OrdinalIgnoreCase));

        return Sale.Reconstitute(
            LegacySalesIds.Sale(row.Id),
            tenantIdResult.Value,
            cashSessionId,
            operatorId,
            paymentTerms,
            customerId,
            paymentMethodId,
            row.Subtotal ?? row.Amount,
            discountResult.Value,
            row.Amount,
            changeResult.Value,
            row.CommissionAmount ?? 0,
            isCancelled,
            row.CompletedAt ?? row.DueDate,
            isCancelled ? row.CompletedAt : null,
            lines);
    }

    public static LegacyReceivableRow ToRow(Sale sale, int cashSessionLegacyId, int? legacyId = null)
    {
        var customerLegacyId = LegacySalesIds.ParseCustomerLegacyId(sale.CustomerId);
        var paymentLegacyId = LegacySalesIds.ParsePaymentMethodLegacyId(sale.PaymentMethodId);
        var operatorLegacyId = LegacySalesIds.ParseOperatorLegacyId(sale.OperatorUserId);

        return new LegacyReceivableRow
        {
            Id = legacyId ?? LegacySalesIds.ParseSaleLegacyId(sale.Id) ?? 0,
            CompanyId = sale.TenantId.Value,
            Type = sale.PaymentTerms == PaymentTerms.Credit ? "Venda Fiado" : "Venda",
            PersonId = customerLegacyId ?? operatorLegacyId ?? 0,
            Amount = sale.Total,
            DueDate = sale.CompletedAt,
            Paid = sale.PaymentTerms == PaymentTerms.Cash ? "Sim" : "Não",
            CashSessionLegacyId = cashSessionLegacyId,
            PaymentMethodLegacyId = paymentLegacyId,
            CustomerLegacyId = customerLegacyId,
            Subtotal = sale.Subtotal,
            Discount = sale.Discount.Value,
            ChangeAmount = sale.Change.Value,
            CommissionAmount = sale.CommissionAmount,
            OperatorLegacyUserId = operatorLegacyId,
            Cancelled = sale.IsCancelled ? "Sim" : "Não",
            CompletedAt = sale.CompletedAt,
        };
    }

    private static Guid LegacyOperatorUserId(int legacyId) =>
        Mappers.StoreSettings.LegacyStoreSettingsIds.User(legacyId);
}
