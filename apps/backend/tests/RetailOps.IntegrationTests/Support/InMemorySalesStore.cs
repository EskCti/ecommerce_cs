using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Application.Queries;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Infrastructure.Legacy.Sales;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.IntegrationTests.Support;

internal sealed class SalesTestState
{
    public Dictionary<Guid, CashSession> Sessions { get; } = new();
    public Dictionary<Guid, Sale> Sales { get; } = new();
    public int SessionCounter { get; set; }
    public int LineCounter { get; set; }
    public int SaleCounter { get; set; }

    public void Reset()
    {
        Sessions.Clear();
        Sales.Clear();
        SessionCounter = 0;
        LineCounter = 0;
        SaleCounter = 0;
    }
}

internal sealed class InMemoryCashSessionRepository(SalesTestState state) : ICashSessionRepository
{
    public Task<Result<CashSession>> GetById(Guid id) =>
        Task.FromResult(state.Sessions.TryGetValue(id, out var session)
            ? Result<CashSession>.Success(session)
            : Result<CashSession>.Failure("Cash session not found."));

    public Task<Result<CashSession?>> GetOpenByOperator(TenantId tenantId, Guid operatorUserId)
    {
        var match = state.Sessions.Values.FirstOrDefault(s =>
            s.TenantId.Value == tenantId.Value
            && s.OperatorUserId == operatorUserId
            && s.Status == CashSessionStatus.Open);

        return Task.FromResult(Result<CashSession?>.Success(match));
    }

    public Task<Result<CashSession?>> GetOpenByTerminal(TenantId tenantId, Guid terminalId)
    {
        var match = state.Sessions.Values.FirstOrDefault(s =>
            s.TenantId.Value == tenantId.Value
            && s.TerminalId == terminalId
            && s.Status == CashSessionStatus.Open);

        return Task.FromResult(Result<CashSession?>.Success(match));
    }

    public Task<Result> Save(CashSession entity)
    {
        if (!state.Sessions.ContainsKey(entity.Id))
        {
            state.SessionCounter++;
            entity.SyncIdentity(Guid.Parse($"00000000-0000-0000-0015-{state.SessionCounter:D12}"));
        }

        state.Sessions[entity.Id] = entity;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Cash session cannot be deleted."));
}

internal sealed class InMemorySaleRepository(SalesTestState state) : ISaleRepository
{
    public Task<Result<Sale>> GetById(Guid id) =>
        Task.FromResult(state.Sales.TryGetValue(id, out var sale)
            ? Result<Sale>.Success(sale)
            : Result<Sale>.Failure("Sale not found."));

    public Task<Result<IReadOnlyList<Sale>>> List(TenantId tenantId, CancellationToken ct = default)
    {
        var items = state.Sales.Values.Where(s => s.TenantId.Value == tenantId.Value).ToList();
        return Task.FromResult(Result<IReadOnlyList<Sale>>.Success(items));
    }

    public Task<Result> Save(Sale entity)
    {
        if (!state.Sales.ContainsKey(entity.Id))
        {
            state.SaleCounter++;
            entity.SyncIdentity(Guid.Parse($"00000000-0000-0000-0016-{state.SaleCounter:D12}"));
        }

        state.Sales[entity.Id] = entity;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Sale cannot be deleted."));
}

internal sealed class InMemorySalesLegacyPort(SalesTestState state) : ISalesLegacyPort
{
    public Task<Result<int>> OpenSession(CashSession session, CancellationToken ct = default) =>
        Task.FromResult(Result<int>.Success(1));

    public Task<Result> SaveSession(CashSession session, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result<int>> AddCartLine(CashSession session, SaleLine line, CancellationToken ct = default)
    {
        if (line.Id == Guid.Empty)
        {
            state.LineCounter++;
            line.SyncIdentity(Guid.Parse($"00000000-0000-0000-0017-{state.LineCounter:D12}"));
        }

        return Task.FromResult(Result<int>.Success(state.LineCounter));
    }

    public Task<Result> RemoveCartLine(CashSession session, Guid lineId, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result<int>> FinalizeSale(CashSession session, Sale sale, CancellationToken ct = default)
    {
        if (sale.Id == Guid.Empty)
        {
            state.SaleCounter++;
            sale.SyncIdentity(Guid.Parse($"00000000-0000-0000-0016-{state.SaleCounter:D12}"));
        }

        return Task.FromResult(Result<int>.Success(state.SaleCounter));
    }

    public Task<Result> CloseSession(CashSession session, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> CancelSale(Sale sale, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

internal sealed class InMemorySalesFixture
{
    public SalesTestState State { get; } = new();
    public InMemoryCashSessionRepository CashSessionRepository { get; }
    public InMemorySaleRepository SaleRepository { get; }
    public InMemorySalesLegacyPort LegacyPort { get; }
    public SalesQueries SalesQueries { get; }

    public InMemorySalesFixture()
    {
        CashSessionRepository = new InMemoryCashSessionRepository(State);
        SaleRepository = new InMemorySaleRepository(State);
        LegacyPort = new InMemorySalesLegacyPort(State);
        SalesQueries = new SalesQueries(SaleRepository);
    }

    public void Reset() => State.Reset();
}
