using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Shared.Kernel.Application.UseCases;

public interface IUseCase<in TIn, TOut>
{
    Task<Result<TOut>> Execute(TIn input);
}

public interface IUseCase<in TIn>
{
    Task<Result> Execute(TIn input);
}
