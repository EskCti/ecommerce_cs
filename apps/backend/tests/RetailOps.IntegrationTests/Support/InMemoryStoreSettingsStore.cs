using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Application.Queries;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.IntegrationTests.Support;

internal sealed class StoreSettingsTestState
{
    public Dictionary<int, StoreConfig> StoreConfigs { get; } = new();
    public Dictionary<Guid, PaymentMethod> PaymentMethods { get; } = new();
    public Dictionary<Guid, CashRegisterTerminal> CashRegisters { get; } = new();
    public int PaymentLegacyId { get; set; }
    public int CashRegisterLegacyId { get; set; }

    public void Reset()
    {
        StoreConfigs.Clear();
        PaymentMethods.Clear();
        CashRegisters.Clear();
        PaymentLegacyId = 0;
        CashRegisterLegacyId = 0;
    }
}

internal sealed class InMemoryStoreConfigRepository(StoreSettingsTestState state) : IStoreConfigRepository
{
    public Task<Result<StoreConfig>> GetById(Guid id)
    {
        var match = state.StoreConfigs.Values.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(match is null
            ? Result<StoreConfig>.Failure("Store config not found.")
            : Result<StoreConfig>.Success(match));
    }

    public Task<Result<StoreConfig>> GetByTenantId(TenantId tenantId) =>
        Task.FromResult(state.StoreConfigs.TryGetValue(tenantId.Value, out var config)
            ? Result<StoreConfig>.Success(config)
            : Result<StoreConfig>.Failure("Store config not found."));

    public Task<Result<bool>> ExistsForTenant(TenantId tenantId) =>
        Task.FromResult(Result<bool>.Success(state.StoreConfigs.ContainsKey(tenantId.Value)));

    public Task<Result> Save(StoreConfig entity)
    {
        entity.SyncIdentity(Guid.Parse($"00000000-0000-0000-0003-{entity.TenantId.Value:D12}"));
        state.StoreConfigs[entity.TenantId.Value] = entity;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Store config cannot be deleted."));
}

internal sealed class InMemoryPaymentMethodRepository(StoreSettingsTestState state) : IPaymentMethodRepository
{
    public Task<Result<PaymentMethod>> GetById(Guid id) =>
        Task.FromResult(state.PaymentMethods.TryGetValue(id, out var method)
            ? Result<PaymentMethod>.Success(method)
            : Result<PaymentMethod>.Failure("Payment method not found."));

    public Task<Result<IEnumerable<PaymentMethod>>> GetByTenantId(TenantId tenantId)
    {
        var items = state.PaymentMethods.Values.Where(p => p.TenantId.Value == tenantId.Value);
        return Task.FromResult(Result<IEnumerable<PaymentMethod>>.Success(items));
    }

    public Task<Result<PaymentMethod>> GetByName(TenantId tenantId, PaymentMethodName name)
    {
        var match = state.PaymentMethods.Values.FirstOrDefault(p =>
            p.TenantId.Value == tenantId.Value
            && p.Name.Value.Equals(name.Value, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(match is null
            ? Result<PaymentMethod>.Failure("Payment method not found.")
            : Result<PaymentMethod>.Success(match));
    }

    public Task<Result<bool>> NameExists(TenantId tenantId, PaymentMethodName name, Guid? excludeId = null)
    {
        var exists = state.PaymentMethods.Values.Any(p =>
            p.TenantId.Value == tenantId.Value
            && p.Name.Value.Equals(name.Value, StringComparison.OrdinalIgnoreCase)
            && (excludeId is null || p.Id != excludeId));

        return Task.FromResult(Result<bool>.Success(exists));
    }

    public Task<Result> Save(PaymentMethod entity)
    {
        if (!state.PaymentMethods.ContainsKey(entity.Id))
        {
            state.PaymentLegacyId++;
            entity.SyncIdentity(Guid.Parse($"00000000-0000-0000-0004-{state.PaymentLegacyId:D12}"));
        }

        state.PaymentMethods[entity.Id] = entity;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(state.PaymentMethods.Remove(id)
            ? Result.Success()
            : Result.Failure("Payment method not found."));
}

internal sealed class InMemoryCashRegisterTerminalRepository(StoreSettingsTestState state) : ICashRegisterTerminalRepository
{
    public Task<Result<CashRegisterTerminal>> GetById(Guid id) =>
        Task.FromResult(state.CashRegisters.TryGetValue(id, out var terminal)
            ? Result<CashRegisterTerminal>.Success(terminal)
            : Result<CashRegisterTerminal>.Failure("Cash register terminal not found."));

    public Task<Result<IEnumerable<CashRegisterTerminal>>> GetByTenantId(TenantId tenantId)
    {
        var items = state.CashRegisters.Values.Where(t => t.TenantId.Value == tenantId.Value);
        return Task.FromResult(Result<IEnumerable<CashRegisterTerminal>>.Success(items));
    }

    public Task<Result<IEnumerable<CashRegisterTerminal>>> GetByStatus(TenantId tenantId, TerminalStatus status)
    {
        var items = state.CashRegisters.Values.Where(t =>
            t.TenantId.Value == tenantId.Value && t.Status == status);
        return Task.FromResult(Result<IEnumerable<CashRegisterTerminal>>.Success(items));
    }

    public Task<Result<CashRegisterTerminal>> GetByName(TenantId tenantId, CashRegisterTerminalName name)
    {
        var match = state.CashRegisters.Values.FirstOrDefault(t =>
            t.TenantId.Value == tenantId.Value
            && t.Name.Value.Equals(name.Value, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(match is null
            ? Result<CashRegisterTerminal>.Failure("Cash register terminal not found.")
            : Result<CashRegisterTerminal>.Success(match));
    }

    public Task<Result> Save(CashRegisterTerminal entity)
    {
        if (!state.CashRegisters.ContainsKey(entity.Id))
        {
            state.CashRegisterLegacyId++;
            entity.SyncIdentity(Guid.Parse($"00000000-0000-0000-0005-{state.CashRegisterLegacyId:D12}"));
        }

        state.CashRegisters[entity.Id] = entity;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(state.CashRegisters.Remove(id)
            ? Result.Success()
            : Result.Failure("Cash register terminal not found."));
}

internal sealed class InMemoryPaymentMethodQueries(StoreSettingsTestState state) : IPaymentMethodQueries
{
    public Task<Result<IReadOnlyList<PaymentMethodListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        PaymentMethodListFilter? filter = null,
        CancellationToken ct = default)
    {
        var items = state.PaymentMethods.Values
            .Where(p => p.TenantId.Value == tenantId.Value)
            .Select(PaymentMethodListItemDto.FromDomain)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<PaymentMethodListItemDto>>.Success(items));
    }

    public Task<Result<PaymentMethodListItemDto?>> GetByIdAsync(
        TenantId tenantId,
        Guid paymentMethodId,
        CancellationToken ct = default)
    {
        if (!state.PaymentMethods.TryGetValue(paymentMethodId, out var method)
            || method.TenantId.Value != tenantId.Value)
        {
            return Task.FromResult(Result<PaymentMethodListItemDto?>.Success(null));
        }

        return Task.FromResult(Result<PaymentMethodListItemDto?>.Success(
            PaymentMethodListItemDto.FromDomain(method)));
    }
}

internal sealed class InMemoryCashRegisterTerminalQueries(StoreSettingsTestState state) : ICashRegisterTerminalQueries
{
    public Task<Result<IReadOnlyList<CashRegisterTerminalListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        CashRegisterTerminalListFilter? filter = null,
        CancellationToken ct = default)
    {
        var items = state.CashRegisters.Values
            .Where(t => t.TenantId.Value == tenantId.Value)
            .Select(t => CashRegisterTerminalListItemDto.FromDomain(t))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CashRegisterTerminalListItemDto>>.Success(items));
    }

    public Task<Result<CashRegisterTerminalListItemDto?>> GetByIdAsync(
        TenantId tenantId,
        Guid terminalId,
        CancellationToken ct = default)
    {
        if (!state.CashRegisters.TryGetValue(terminalId, out var terminal)
            || terminal.TenantId.Value != tenantId.Value)
        {
            return Task.FromResult(Result<CashRegisterTerminalListItemDto?>.Success(null));
        }

        return Task.FromResult(Result<CashRegisterTerminalListItemDto?>.Success(
            CashRegisterTerminalListItemDto.FromDomain(terminal)));
    }
}

internal sealed class InMemoryStoreSettingsFixture
{
    public StoreSettingsTestState State { get; } = new();
    public InMemoryStoreConfigRepository StoreConfigRepository { get; }
    public InMemoryPaymentMethodRepository PaymentMethodRepository { get; }
    public InMemoryCashRegisterTerminalRepository CashRegisterTerminalRepository { get; }
    public InMemoryPaymentMethodQueries PaymentMethodQueries { get; }
    public InMemoryCashRegisterTerminalQueries CashRegisterTerminalQueries { get; }

    public InMemoryStoreSettingsFixture()
    {
        StoreConfigRepository = new InMemoryStoreConfigRepository(State);
        PaymentMethodRepository = new InMemoryPaymentMethodRepository(State);
        CashRegisterTerminalRepository = new InMemoryCashRegisterTerminalRepository(State);
        PaymentMethodQueries = new InMemoryPaymentMethodQueries(State);
        CashRegisterTerminalQueries = new InMemoryCashRegisterTerminalQueries(State);
    }

    public void Reset() => State.Reset();
}
