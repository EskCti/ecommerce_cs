using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.Catalog;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Catalog;

public sealed class LegacyProductRepository(
    ICatalogLegacyPort legacyPort,
    LegacySasDbContext db) : IProductRepository
{
    public async Task<Result<Product>> GetById(Guid id)
    {
        var legacyId = LegacyCatalogIds.ParseLegacyId(id, "0009");
        if (legacyId is null)
            return Result<Product>.Failure("Invalid product id.");

        var row = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == legacyId);

        if (row is null)
            return Result<Product>.Failure("Product not found.");

        var tenantId = TenantId.Create(row.CompanyId);
        if (tenantId.IsFailure)
            return Result<Product>.Failure(tenantId.Error);

        var result = await legacyPort.GetProductFromLegacyAsync(tenantId.Value, id);
        return result.IsFailure
            ? Result<Product>.Failure(result.Error)
            : result.Value is null
                ? Result<Product>.Failure("Product not found.")
                : Result<Product>.Success(result.Value);
    }

    public async Task<Result<Product?>> FindByBarcode(TenantId tenantId, Barcode barcode)
    {
        var result = await legacyPort.GetProductByBarcodeFromLegacyAsync(tenantId, barcode.Value);
        return result.IsFailure
            ? Result<Product?>.Failure(result.Error)
            : Result<Product?>.Success(result.Value);
    }

    public async Task<Result<bool>> ExistsBarcode(
        TenantId tenantId,
        Barcode barcode,
        Guid? excludeProductId = null)
    {
        var existingResult = await FindByBarcode(tenantId, barcode);
        if (existingResult.IsFailure)
            return Result<bool>.Failure(existingResult.Error);

        if (existingResult.Value is null)
            return Result<bool>.Success(false);

        if (excludeProductId is not null && existingResult.Value.Id == excludeProductId)
            return Result<bool>.Success(false);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<Product>>> GetByTenantId(TenantId tenantId)
    {
        var result = await legacyPort.GetProductsFromLegacyAsync(tenantId);
        return result.IsFailure
            ? Result<IEnumerable<Product>>.Failure(result.Error)
            : Result<IEnumerable<Product>>.Success(result.Value);
    }

    public async Task<Result> Save(Product entity)
    {
        var result = await legacyPort.SaveProductToLegacyAsync(entity);
        return result.IsFailure ? Result.Failure(result.Error) : Result.Success();
    }

    public async Task<Result<(Product Product, GradeVariant Variant)>> FindVariantById(Guid variantId)
    {
        var legacyVariantId = LegacyCatalogIds.ParseLegacyId(variantId, "0015");
        if (legacyVariantId is null)
            return Result<(Product, GradeVariant)>.Failure("Invalid grade variant id.");

        var variantRow = await db.GradeVariants.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == legacyVariantId);

        if (variantRow is null)
            return Result<(Product, GradeVariant)>.Failure("Grade variant not found.");

        var productId = LegacyCatalogIds.Product(variantRow.ProductLegacyId);
        var productResult = await GetById(productId);
        if (productResult.IsFailure)
            return Result<(Product, GradeVariant)>.Failure(productResult.Error);

        var variant = productResult.Value.GradeVariants.FirstOrDefault(v => v.Id == variantId);
        return variant is null
            ? Result<(Product, GradeVariant)>.Failure("Grade variant not found.")
            : Result<(Product, GradeVariant)>.Success((productResult.Value, variant));
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Product cannot be deleted. Use deactivation instead."));
}
