using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed class DeactivateProductUseCase(IProductRepository productRepository)
    : IUseCase<(int tenantId, Guid productId), ProductOutputDto>
{
    public async Task<Result<ProductOutputDto>> Execute(
        (int tenantId, Guid productId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, productId) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<ProductOutputDto>.Failure(tenantIdResult.Error);

        var productResult = await productRepository.GetById(productId);
        if (productResult.IsFailure)
            return Result<ProductOutputDto>.Failure(productResult.Error);

        var product = productResult.Value;
        if (product.TenantId.Value != tenantId)
            return Result<ProductOutputDto>.Failure("Product does not belong to this tenant.");

        var deactivateResult = product.Deactivate();
        if (deactivateResult.IsFailure)
            return Result<ProductOutputDto>.Failure(deactivateResult.Error);

        var saveResult = await productRepository.Save(product);
        if (saveResult.IsFailure)
            return Result<ProductOutputDto>.Failure(saveResult.Error);

        return Result<ProductOutputDto>.Success(ProductOutputDto.FromDomain(product));
    }
}

public sealed class GetProductUseCase(IProductRepository productRepository)
    : IUseCase<(int tenantId, Guid productId), ProductOutputDto>
{
    public async Task<Result<ProductOutputDto>> Execute(
        (int tenantId, Guid productId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, productId) = request;

        var productResult = await productRepository.GetById(productId);
        if (productResult.IsFailure)
            return Result<ProductOutputDto>.Failure(productResult.Error);

        if (productResult.Value.TenantId.Value != tenantId)
            return Result<ProductOutputDto>.Failure("Product does not belong to this tenant.");

        return Result<ProductOutputDto>.Success(ProductOutputDto.FromDomain(productResult.Value));
    }
}
