using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Crm.Application.UseCases;

public interface IUseCase<TInput, TOutput>
{
    Task<Result<TOutput>> Execute(TInput input, CancellationToken cancellationToken = default);
}
