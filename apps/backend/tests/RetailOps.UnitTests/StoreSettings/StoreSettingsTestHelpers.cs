using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.UnitTests.StoreSettings;

internal static class StoreSettingsTestHelpers
{
    internal static TenantId Tenant(int id = 1) => TenantId.Create(id).Value;

    internal static Guid PaymentMethodId(int legacyId = 1) =>
        Guid.Parse($"00000000-0000-0000-0004-{legacyId:D12}");

    internal static Guid CashRegisterId(int legacyId = 1) =>
        Guid.Parse($"00000000-0000-0000-0005-{legacyId:D12}");

    internal static Guid StoreConfigId(int tenantId = 1) =>
        Guid.Parse($"00000000-0000-0000-0003-{tenantId:D12}");

    internal static StoreConfig CreateStoreConfig(int tenantId = 1)
    {
        var config = StoreConfig.Create(
            Tenant(tenantId),
            StoreName.Create("Loja Teste").Value,
            cnpj: null,
            DiscountType.Percentage,
            discountValue: 10,
            CommissionRate.Create(5).Value,
            ReportFormat.Create("PDF").Value,
            contacts: "contato@test.com",
            address: "Rua A, 1").Value;

        config.SyncIdentity(StoreConfigId(tenantId));
        return config;
    }

    internal static PaymentMethod CreatePaymentMethod(
        int tenantId = 1,
        int legacyId = 1,
        string name = "Pix",
        decimal surcharge = 2.5m,
        bool isActive = true)
    {
        return PaymentMethod.Reconstitute(
            PaymentMethodId(legacyId),
            Tenant(tenantId),
            PaymentMethodName.Create(name).Value,
            SurchargePercent.Create(surcharge).Value,
            isActive,
            DateTime.UtcNow,
            DateTime.UtcNow).Value;
    }

    internal static CashRegisterTerminal CreateCashRegister(
        int tenantId = 1,
        int legacyId = 1,
        string name = "Caixa 1",
        TerminalStatus status = TerminalStatus.Closed)
    {
        return CashRegisterTerminal.Reconstitute(
            CashRegisterId(legacyId),
            Tenant(tenantId),
            CashRegisterTerminalName.Create(name).Value,
            status,
            assignedOperatorId: null,
            DateTime.UtcNow,
            DateTime.UtcNow).Value;
    }
}
