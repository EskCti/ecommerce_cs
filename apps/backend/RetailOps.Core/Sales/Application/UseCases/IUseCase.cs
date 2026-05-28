using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Sales.Application.UseCases;

public interface IUseCase<TInput, TOutput>
{
    Task<Result<TOutput>> Execute(TInput input, CancellationToken cancellationToken = default);
}
