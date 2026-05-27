using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed class GenerateBarcodeUseCase(IProductRepository productRepository)
    : IUseCase<int, GenerateBarcodeOutputDto>
{
    public async Task<Result<GenerateBarcodeOutputDto>> Execute(
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<GenerateBarcodeOutputDto>.Failure(tenantIdResult.Error);

        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = GenerateCandidate();
            var barcodeResult = Barcode.Create(candidate);
            if (barcodeResult.IsFailure)
                continue;

            var existsResult = await productRepository.ExistsBarcode(
                tenantIdResult.Value,
                barcodeResult.Value);

            if (existsResult.IsFailure)
                return Result<GenerateBarcodeOutputDto>.Failure(existsResult.Error);

            if (!existsResult.Value)
                return Result<GenerateBarcodeOutputDto>.Success(new GenerateBarcodeOutputDto { Barcode = candidate });
        }

        return Result<GenerateBarcodeOutputDto>.Failure("Unable to generate a unique barcode.");
    }

    private static string GenerateCandidate() =>
        $"{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";
}
