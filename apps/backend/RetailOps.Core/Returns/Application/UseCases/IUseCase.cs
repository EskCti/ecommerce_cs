namespace RetailOps.Core.Returns.Application.UseCases;

public interface IUseCase<in TRequest, TResponse>
{
    Task<RetailOps.Shared.Kernel.Domain.Results.Result<TResponse>> Execute(
        TRequest request,
        CancellationToken cancellationToken = default);
}
