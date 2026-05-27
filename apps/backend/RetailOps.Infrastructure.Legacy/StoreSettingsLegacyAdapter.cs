using Microsoft.EntityFrameworkCore;
using RetailOps.Core.StoreSettings.Application.Ports;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy;

public sealed class StoreSettingsLegacyAdapter(LegacySasDbContext db) : IStoreSettingsLegacyPort
{
    public async Task<Result<StoreConfig?>> GetStoreConfigFromLegacyAsync(TenantId tenantId, CancellationToken ct = default)
    {
        try
        {
            var row = await db.Configs.AsNoTracking()
                .FirstOrDefaultAsync(r => r.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result<StoreConfig?>.Success(null);

            var result = LegacyStoreConfigMapper.ToDomain(row);
            return result.IsFailure
                ? Result<StoreConfig?>.Failure(result.Error)
                : Result<StoreConfig?>.Success(result.Value);
        }
        catch (Exception ex)
        {
            return Result<StoreConfig?>.Failure($"Failed to get StoreConfig from legacy: {ex.Message}");
        }
    }

    public async Task<Result> SaveStoreConfigToLegacyAsync(StoreConfig storeConfig, CancellationToken ct = default)
    {
        try
        {
            var rowResult = LegacyStoreConfigMapper.ToLegacy(storeConfig);
            if (rowResult.IsFailure)
                return Result.Failure(rowResult.Error);

            var row = rowResult.Value;
            var existing = await db.Configs
                .FirstOrDefaultAsync(r => r.CompanyId == storeConfig.TenantId.Value, ct);

            if (existing is null)
            {
                db.Configs.Add(row);
            }
            else
            {
                existing.NomeSistema = row.NomeSistema;
                existing.Contatos = row.Contatos;
                existing.CnpjSistema = row.CnpjSistema;
                existing.Endereco = row.Endereco;
                existing.TipoRel = row.TipoRel;
                existing.TipoDesconto = row.TipoDesconto;
                existing.Comissao = row.Comissao;
                existing.Token = row.Token;
                existing.FotoRel = row.FotoRel;
            }

            await db.SaveChangesAsync(ct);
            storeConfig.SyncIdentity(LegacyStoreSettingsIds.StoreConfig(storeConfig.TenantId.Value));
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to save StoreConfig to legacy: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<PaymentMethod>>> GetPaymentMethodsFromLegacyAsync(TenantId tenantId, CancellationToken ct = default)
    {
        try
        {
            var rows = await db.PaymentMethods.AsNoTracking()
                .Where(r => r.CompanyId == tenantId.Value)
                .OrderBy(r => r.Nome)
                .ToListAsync(ct);

            var paymentMethods = new List<PaymentMethod>();
            foreach (var row in rows)
            {
                var result = LegacyPaymentMethodMapper.ToDomain(row);
                if (result.IsSuccess)
                    paymentMethods.Add(result.Value);
            }

            return Result<IReadOnlyList<PaymentMethod>>.Success(paymentMethods);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PaymentMethod>>.Failure($"Failed to get PaymentMethods from legacy: {ex.Message}");
        }
    }

    public async Task<Result<PaymentMethod?>> GetPaymentMethodFromLegacyAsync(TenantId tenantId, Guid paymentMethodId, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyStoreSettingsIds.ParseLegacyId(paymentMethodId, "0004");
            if (legacyId is null)
                return Result<PaymentMethod?>.Success(null);

            var row = await db.PaymentMethods.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result<PaymentMethod?>.Success(null);

            var result = LegacyPaymentMethodMapper.ToDomain(row);
            return result.IsFailure
                ? Result<PaymentMethod?>.Failure(result.Error)
                : Result<PaymentMethod?>.Success(result.Value);
        }
        catch (Exception ex)
        {
            return Result<PaymentMethod?>.Failure($"Failed to get PaymentMethod from legacy: {ex.Message}");
        }
    }

    public async Task<Result<int>> SavePaymentMethodToLegacyAsync(PaymentMethod paymentMethod, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyStoreSettingsIds.ParseLegacyId(paymentMethod.Id, "0004");
            var rowResult = LegacyPaymentMethodMapper.ToLegacy(paymentMethod, legacyId);
            if (rowResult.IsFailure)
                return Result<int>.Failure(rowResult.Error);

            var row = rowResult.Value;

            if (legacyId is > 0)
            {
                var existing = await db.PaymentMethods
                    .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == paymentMethod.TenantId.Value, ct);

                if (existing is not null)
                {
                    existing.Nome = row.Nome;
                    existing.Acrescimo = row.Acrescimo;
                    existing.Ativo = row.Ativo;
                    await db.SaveChangesAsync(ct);
                    paymentMethod.SyncIdentity(LegacyStoreSettingsIds.PaymentMethod(existing.Id));
                    return Result<int>.Success(existing.Id);
                }
            }

            db.PaymentMethods.Add(row);
            await db.SaveChangesAsync(ct);
            paymentMethod.SyncIdentity(LegacyStoreSettingsIds.PaymentMethod(row.Id));
            return Result<int>.Success(row.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save PaymentMethod to legacy: {ex.Message}");
        }
    }

    public async Task<Result> DeletePaymentMethodFromLegacyAsync(TenantId tenantId, Guid paymentMethodId, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyStoreSettingsIds.ParseLegacyId(paymentMethodId, "0004");
            if (legacyId is null)
                return Result.Failure("Invalid payment method id.");

            var row = await db.PaymentMethods
                .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result.Failure("Payment method not found.");

            db.PaymentMethods.Remove(row);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete PaymentMethod from legacy: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<CashRegisterTerminal>>> GetCashRegisterTerminalsFromLegacyAsync(TenantId tenantId, CancellationToken ct = default)
    {
        try
        {
            var rows = await db.CashRegisters.AsNoTracking()
                .Where(r => r.CompanyId == tenantId.Value)
                .OrderBy(r => r.Nome)
                .ToListAsync(ct);

            var terminals = new List<CashRegisterTerminal>();
            foreach (var row in rows)
            {
                var result = LegacyCashRegisterTerminalMapper.ToDomain(row);
                if (result.IsSuccess)
                    terminals.Add(result.Value);
            }

            return Result<IReadOnlyList<CashRegisterTerminal>>.Success(terminals);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CashRegisterTerminal>>.Failure($"Failed to get CashRegisterTerminals from legacy: {ex.Message}");
        }
    }

    public async Task<Result<CashRegisterTerminal?>> GetCashRegisterTerminalFromLegacyAsync(TenantId tenantId, Guid terminalId, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyStoreSettingsIds.ParseLegacyId(terminalId, "0005");
            if (legacyId is null)
                return Result<CashRegisterTerminal?>.Success(null);

            var row = await db.CashRegisters.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result<CashRegisterTerminal?>.Success(null);

            var result = LegacyCashRegisterTerminalMapper.ToDomain(row);
            return result.IsFailure
                ? Result<CashRegisterTerminal?>.Failure(result.Error)
                : Result<CashRegisterTerminal?>.Success(result.Value);
        }
        catch (Exception ex)
        {
            return Result<CashRegisterTerminal?>.Failure($"Failed to get CashRegisterTerminal from legacy: {ex.Message}");
        }
    }

    public async Task<Result<int>> SaveCashRegisterTerminalToLegacyAsync(CashRegisterTerminal terminal, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyStoreSettingsIds.ParseLegacyId(terminal.Id, "0005");
            var rowResult = LegacyCashRegisterTerminalMapper.ToLegacy(terminal, legacyId);
            if (rowResult.IsFailure)
                return Result<int>.Failure(rowResult.Error);

            var row = rowResult.Value;

            if (legacyId is > 0)
            {
                var existing = await db.CashRegisters
                    .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == terminal.TenantId.Value, ct);

                if (existing is not null)
                {
                    existing.Nome = row.Nome;
                    existing.Status = row.Status;
                    existing.Operador = row.Operador;
                    await db.SaveChangesAsync(ct);
                    terminal.SyncIdentity(LegacyStoreSettingsIds.CashRegisterTerminal(existing.Id));
                    return Result<int>.Success(existing.Id);
                }
            }

            db.CashRegisters.Add(row);
            await db.SaveChangesAsync(ct);
            terminal.SyncIdentity(LegacyStoreSettingsIds.CashRegisterTerminal(row.Id));
            return Result<int>.Success(row.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save CashRegisterTerminal to legacy: {ex.Message}");
        }
    }

    public async Task<Result> DeleteCashRegisterTerminalFromLegacyAsync(TenantId tenantId, Guid terminalId, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyStoreSettingsIds.ParseLegacyId(terminalId, "0005");
            if (legacyId is null)
                return Result.Failure("Invalid cash register terminal id.");

            var row = await db.CashRegisters
                .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result.Failure("Cash register terminal not found.");

            db.CashRegisters.Remove(row);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete CashRegisterTerminal from legacy: {ex.Message}");
        }
    }
}
