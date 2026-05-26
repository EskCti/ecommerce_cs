using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;

public static class LegacyCashRegisterTerminalMapper
{
    public static Result<CashRegisterTerminal> ToDomain(LegacyCashRegisterRow row)
    {
        try
        {
            var tenantIdResult = TenantId.Create(row.CompanyId);
            if (tenantIdResult.IsFailure)
                return Result<CashRegisterTerminal>.Failure(tenantIdResult.Error);

            if (string.IsNullOrWhiteSpace(row.Nome))
                return Result<CashRegisterTerminal>.Failure("Cash register terminal name is required.");

            var nameResult = CashRegisterTerminalName.Create(row.Nome);
            if (nameResult.IsFailure)
                return Result<CashRegisterTerminal>.Failure(nameResult.Error);

            var status = TerminalStatusExtensions.FromLegacyValue(row.Status);

            UserId? assignedOperatorId = null;
            if (row.Operador is > 0)
            {
                var operatorIdResult = UserId.Create(LegacyStoreSettingsIds.User(row.Operador.Value));
                if (operatorIdResult.IsSuccess)
                    assignedOperatorId = operatorIdResult.Value;
            }

            return CashRegisterTerminal.Reconstitute(
                LegacyStoreSettingsIds.CashRegisterTerminal(row.Id),
                tenantIdResult.Value,
                nameResult.Value,
                status,
                assignedOperatorId,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            return Result<CashRegisterTerminal>.Failure($"Failed to map legacy CashRegisterTerminal: {ex.Message}");
        }
    }

    public static Result<LegacyCashRegisterRow> ToLegacy(CashRegisterTerminal terminal, int? legacyId = null)
    {
        try
        {
            int? operador = null;
            if (terminal.AssignedOperatorId is { IsEmpty: false } operatorId)
            {
                var suffix = operatorId.Value.ToString().Split('-').Last();
                if (int.TryParse(suffix, out var legacyUserId))
                    operador = legacyUserId;
            }

            var row = new LegacyCashRegisterRow
            {
                Id = legacyId ?? LegacyStoreSettingsIds.ParseLegacyId(terminal.Id, "0005") ?? 0,
                CompanyId = terminal.TenantId.Value,
                Nome = terminal.Name.Value,
                Status = terminal.Status.ToLegacyValue(),
                Operador = operador
            };

            return Result<LegacyCashRegisterRow>.Success(row);
        }
        catch (Exception ex)
        {
            return Result<LegacyCashRegisterRow>.Failure($"Failed to map CashRegisterTerminal to legacy: {ex.Message}");
        }
    }
}
