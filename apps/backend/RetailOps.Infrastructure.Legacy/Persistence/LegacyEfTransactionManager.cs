using Microsoft.EntityFrameworkCore;
using RetailOps.Shared.Kernel.Domain.Transactions;

namespace RetailOps.Infrastructure.Legacy.Persistence;

public sealed class LegacyEfTransactionManager(LegacySasDbContext dbContext) : ITransactionManager
{
    public async Task<T> RunInTransactionAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
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

    public Task RunInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default) =>
        RunInTransactionAsync(async () =>
        {
            await action();
            return true;
        }, cancellationToken);
}
