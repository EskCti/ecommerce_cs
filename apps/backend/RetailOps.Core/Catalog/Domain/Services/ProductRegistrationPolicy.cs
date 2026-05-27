using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Domain.Services;

public sealed class ProductRegistrationPolicy(IProductRepository productRepository)
{
    public async Task<Result> ValidateUniqueBarcodeAsync(
        TenantId tenantId,
        Barcode barcode,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        var existsResult = await productRepository.ExistsBarcode(tenantId, barcode, excludeProductId);
        if (existsResult.IsFailure)
            return Result.Failure(existsResult.Error);

        if (existsResult.Value)
            return Result.Failure($"DUPLICATE_BARCODE: Barcode '{barcode.Value}' is already registered for this tenant.");

        return Result.Success();
    }
}
