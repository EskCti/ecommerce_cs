using RetailOps.Core.Returns.Application.Ports;
using RetailOps.Core.Returns.Domain.Entities;
using RetailOps.Core.Returns.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.IntegrationTests.Support;

internal sealed class ReturnsTestState
{
    public Dictionary<Guid, Exchange> Exchanges { get; } = new();
    public int ExchangeLegacyCounter { get; set; }

    public void Reset()
    {
        Exchanges.Clear();
        ExchangeLegacyCounter = 0;
    }
}

internal static class ReturnsTestIds
{
    internal static Guid Exchange(int legacyId) =>
        Guid.Parse($"00000000-0000-0000-0022-{legacyId:D12}");

    internal static int? ParseExchangeLegacyId(Guid id)
    {
        var parts = id.ToString().Split('-');
        if (parts.Length != 5 || parts[3] != "0022")
            return null;

        return int.TryParse(parts[4], out var legacyId) ? legacyId : null;
    }
}

internal sealed class InMemoryExchangeRepository(ReturnsTestState state) : IExchangeRepository, IReturnsLegacyPort
{
    public Task<Result<Exchange>> GetById(TenantId tenantId, Guid id, CancellationToken ct = default)
    {
        if (!state.Exchanges.TryGetValue(id, out var exchange))
            return Task.FromResult(Result<Exchange>.Failure("Exchange not found."));

        if (exchange.TenantId.Value != tenantId.Value)
            return Task.FromResult(Result<Exchange>.Failure("Exchange not found."));

        return Task.FromResult(Result<Exchange>.Success(exchange));
    }

    public Task<Result<IReadOnlyList<Exchange>>> ListByTenant(TenantId tenantId, CancellationToken ct = default)
    {
        var items = state.Exchanges.Values
            .Where(e => e.TenantId.Value == tenantId.Value)
            .OrderByDescending(e => e.RegisteredAt)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<Exchange>>.Success(items));
    }

    public Task<Result> Save(Exchange exchange, CancellationToken ct = default)
    {
        if (!state.Exchanges.ContainsKey(exchange.Id))
        {
            state.ExchangeLegacyCounter++;
            exchange.SyncIdentity(ReturnsTestIds.Exchange(state.ExchangeLegacyCounter));
        }

        state.Exchanges[exchange.Id] = exchange;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(TenantId tenantId, Guid id, CancellationToken ct = default)
    {
        if (!state.Exchanges.TryGetValue(id, out var exchange) || exchange.TenantId.Value != tenantId.Value)
            return Task.FromResult(Result.Failure("Exchange not found."));

        state.Exchanges.Remove(id);
        return Task.FromResult(Result.Success());
    }

    public async Task<Result<int>> SaveExchangeToLegacyAsync(Exchange exchange, CancellationToken ct = default)
    {
        var saveResult = await Save(exchange, ct);
        if (saveResult.IsFailure)
            return Result<int>.Failure(saveResult.Error);

        var legacyId = ReturnsTestIds.ParseExchangeLegacyId(exchange.Id) ?? 0;
        return Result<int>.Success(legacyId);
    }

    public Task<Result> DeleteExchangeFromLegacyAsync(TenantId tenantId, Guid exchangeId, CancellationToken ct = default) =>
        Delete(tenantId, exchangeId, ct);
}

internal sealed class InMemoryReturnsFixture
{
    public ReturnsTestState State { get; } = new();
    public InMemoryExchangeRepository ExchangeRepository { get; }

    public InMemoryReturnsFixture()
    {
        ExchangeRepository = new InMemoryExchangeRepository(State);
    }

    public void Reset() => State.Reset();
}
