using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Application.UseCases;

public interface IUseCase<TInput, TOutput>
{
    Task<Result<TOutput>> Execute(TInput input, CancellationToken cancellationToken = default);
}

public static class CatalogErrors
{
    public const string DuplicateBarcode = "DUPLICATE_BARCODE";
}
