using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Sales;

internal static class LegacyCashSessionMapper
{
    public static Result<CashSession> ToDomain(
        LegacyCashSessionRow row,
        IEnumerable<SaleLine> lines,
        IEnumerable<CashWithdrawal> withdrawals)
    {
        var tenantIdResult = TenantId.Create(row.CompanyId);
        if (tenantIdResult.IsFailure)
            return Result<CashSession>.Failure(tenantIdResult.Error);

        var terminalId = LegacyStoreSettingsTerminalId(row.TerminalLegacyId);
        var operatorId = LegacyOperatorUserId(row.OperatorLegacyUserId);

        var status = row.Status.Equals("Fechado", StringComparison.OrdinalIgnoreCase)
            ? CashSessionStatus.Closed
            : CashSessionStatus.Open;

        CashBreakage? breakage = null;
        if (row.Breakage is not null)
        {
            var breakageResult = CashBreakage.Create(row.Breakage.Value);
            if (breakageResult.IsFailure)
                return Result<CashSession>.Failure(breakageResult.Error);
            breakage = breakageResult.Value;
        }

        return CashSession.Reconstitute(
            LegacySalesIds.CashSession(row.Id),
            tenantIdResult.Value,
            terminalId,
            operatorId,
            status,
            row.OpeningFloat,
            row.TotalSold,
            row.CountedCash,
            breakage,
            row.OpenedAt,
            row.ClosedAt,
            lines,
            withdrawals);
    }

    public static LegacyCashSessionRow ToRow(CashSession session, int? legacyId = null)
    {
        var terminalLegacyId = LegacySalesIds.ParseTerminalLegacyId(session.TerminalId) ?? 0;
        var operatorLegacyId = LegacySalesIds.ParseOperatorLegacyId(session.OperatorUserId) ?? 0;

        return new LegacyCashSessionRow
        {
            Id = legacyId ?? LegacySalesIds.ParseCashSessionLegacyId(session.Id) ?? 0,
            CompanyId = session.TenantId.Value,
            TerminalLegacyId = terminalLegacyId,
            OperatorLegacyUserId = operatorLegacyId,
            Status = session.Status == CashSessionStatus.Open ? "Aberto" : "Fechado",
            OpeningFloat = session.OpeningFloat,
            TotalSold = session.TotalSold,
            CountedCash = session.CountedCash,
            Breakage = session.Breakage?.Value,
            OpenedAt = session.OpenedAt,
            ClosedAt = session.ClosedAt,
        };
    }

    private static Guid LegacyStoreSettingsTerminalId(int legacyId) =>
        Mappers.StoreSettings.LegacyStoreSettingsIds.CashRegisterTerminal(legacyId);

    private static Guid LegacyOperatorUserId(int legacyId) =>
        Mappers.StoreSettings.LegacyStoreSettingsIds.User(legacyId);
}
