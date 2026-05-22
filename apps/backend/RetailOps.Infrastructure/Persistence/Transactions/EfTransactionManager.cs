using Microsoft.EntityFrameworkCore;
using RetailOps.Infrastructure.Persistence.Contexts;
using RetailOps.Shared.Kernel.Domain.Transactions;

namespace RetailOps.Infrastructure.Persistence.Transactions;

public sealed class EfTransactionManager : ITransactionManager
{
    private readonly RetailOpsDbContext _dbContext;

    public EfTransactionManager(RetailOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> RunInTransactionAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RunInTransactionAsync(
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        await RunInTransactionAsync(async () =>
        {
            await action();
            return true;
        }, cancellationToken);
    }
}
