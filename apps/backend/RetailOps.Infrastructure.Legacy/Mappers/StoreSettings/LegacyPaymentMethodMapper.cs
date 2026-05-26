using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;

public static class LegacyPaymentMethodMapper
{
    public static Result<PaymentMethod> ToDomain(LegacyPaymentMethodRow row)
    {
        try
        {
            var tenantIdResult = TenantId.Create(row.CompanyId);
            if (tenantIdResult.IsFailure)
                return Result<PaymentMethod>.Failure(tenantIdResult.Error);

            if (string.IsNullOrWhiteSpace(row.Nome))
                return Result<PaymentMethod>.Failure("Payment method name is required.");

            var nameResult = PaymentMethodName.Create(row.Nome);
            if (nameResult.IsFailure)
                return Result<PaymentMethod>.Failure(nameResult.Error);

            var surchargeResult = SurchargePercent.Create(row.Acrescimo ?? 0);
            if (surchargeResult.IsFailure)
                return Result<PaymentMethod>.Failure(surchargeResult.Error);

            var isActive = row.Ativo is null
                || row.Ativo.Equals("S", StringComparison.OrdinalIgnoreCase)
                || row.Ativo.Equals("Sim", StringComparison.OrdinalIgnoreCase);

            return PaymentMethod.Reconstitute(
                LegacyStoreSettingsIds.PaymentMethod(row.Id),
                tenantIdResult.Value,
                nameResult.Value,
                surchargeResult.Value,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            return Result<PaymentMethod>.Failure($"Failed to map legacy PaymentMethod: {ex.Message}");
        }
    }

    public static Result<LegacyPaymentMethodRow> ToLegacy(PaymentMethod paymentMethod, int? legacyId = null)
    {
        try
        {
            var row = new LegacyPaymentMethodRow
            {
                Id = legacyId ?? LegacyStoreSettingsIds.ParseLegacyId(paymentMethod.Id, "0004") ?? 0,
                CompanyId = paymentMethod.TenantId.Value,
                Nome = paymentMethod.Name.Value,
                Acrescimo = paymentMethod.SurchargePercent.Value,
                Ativo = paymentMethod.IsActive ? "Sim" : "Não"
            };

            return Result<LegacyPaymentMethodRow>.Success(row);
        }
        catch (Exception ex)
        {
            return Result<LegacyPaymentMethodRow>.Failure($"Failed to map PaymentMethod to legacy: {ex.Message}");
        }
    }
}
