namespace RetailOps.Shared.Kernel.Domain.Transactions;

public interface ITransactionManager
{
    Task<T> RunInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
    Task RunInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
