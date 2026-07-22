using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Returns.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Returns.Domain.Services;

public sealed class ExchangeStockPolicy(
    IProductCatalogService productCatalogService,
    IProductRepository productRepository)
{
    public async Task<Result<int>> GetAvailableOutboundStockAsync(
        TenantId tenantId,
        Guid productOutId,
        GradeSelection? gradeOut,
        CancellationToken ct = default)
    {
        var variantResult = await ResolveVariantIdAsync(tenantId, productOutId, gradeOut, ct);
        if (variantResult.IsFailure)
            return Result<int>.Failure(variantResult.Error);

        if (variantResult.Value is Guid variantId)
            return await productCatalogService.GetVariantStockAsync(tenantId, variantId, ct);

        if (gradeOut?.HasGrade == true)
            return Result<int>.Failure("Grade variant could not be resolved for outbound product.");

        return await productCatalogService.GetAvailableStockAsync(tenantId, productOutId, ct);
    }

    public async Task<Result> ApplyRegisterAsync(
        TenantId tenantId,
        Guid productInId,
        GradeSelection? gradeIn,
        Guid productOutId,
        GradeSelection? gradeOut,
        CancellationToken ct = default)
    {
        var increaseIn = await AdjustAsync(tenantId, productInId, gradeIn, +1, ct);
        if (increaseIn.IsFailure)
            return increaseIn;

        var decreaseOut = await AdjustAsync(tenantId, productOutId, gradeOut, -1, ct);
        if (decreaseOut.IsFailure)
        {
            await AdjustAsync(tenantId, productInId, gradeIn, -1, ct);
            return decreaseOut;
        }

        return Result.Success();
    }

    public async Task<Result> ApplyDeleteReverseAsync(
        TenantId tenantId,
        Guid productInId,
        GradeSelection? gradeIn,
        Guid productOutId,
        GradeSelection? gradeOut,
        CancellationToken ct = default)
    {
        var decreaseIn = await AdjustAsync(tenantId, productInId, gradeIn, -1, ct);
        if (decreaseIn.IsFailure)
            return decreaseIn;

        var increaseOut = await AdjustAsync(tenantId, productOutId, gradeOut, +1, ct);
        if (increaseOut.IsFailure)
        {
            await AdjustAsync(tenantId, productInId, gradeIn, +1, ct);
            return increaseOut;
        }

        return Result.Success();
    }

    private async Task<Result> AdjustAsync(
        TenantId tenantId,
        Guid productId,
        GradeSelection? grade,
        int delta,
        CancellationToken ct)
    {
        var quantity = Math.Abs(delta);
        var variantResult = await ResolveVariantIdAsync(tenantId, productId, grade, ct);
        if (variantResult.IsFailure)
            return Result.Failure(variantResult.Error);

        if (variantResult.Value is Guid variantId)
        {
            return delta > 0
                ? await productCatalogService.ReleaseVariantStockAsync(tenantId, variantId, quantity, ct)
                : await productCatalogService.ReserveVariantStockAsync(tenantId, variantId, quantity, ct);
        }

        if (grade?.HasGrade == true)
            return Result.Failure("Grade variant could not be resolved for stock adjustment.");

        return delta > 0
            ? await productCatalogService.ReleaseStockAsync(tenantId, productId, quantity, ct)
            : await productCatalogService.ReserveStockAsync(tenantId, productId, quantity, ct);
    }

    private async Task<Result<Guid?>> ResolveVariantIdAsync(
        TenantId tenantId,
        Guid productId,
        GradeSelection? grade,
        CancellationToken ct)
    {
        if (grade is null || !grade.HasGrade)
            return Result<Guid?>.Success(null);

        if (grade.VariantId is Guid variantId)
            return Result<Guid?>.Success(variantId);

        if (grade.OptionIds.Count == 0)
            return Result<Guid?>.Failure("Grade option ids are required.");

        var productResult = await productRepository.GetById(productId);
        if (productResult.IsFailure)
            return Result<Guid?>.Failure(productResult.Error);

        if (productResult.Value.TenantId.Value != tenantId.Value)
            return Result<Guid?>.Failure("Product does not belong to this tenant.");

        var variant = productResult.Value.FindVariantByOptions(grade.OptionIds).Value;
        if (variant is null)
            return Result<Guid?>.Failure("Grade variant not found for selected options.");

        return Result<Guid?>.Success(variant.Id);
    }
}
