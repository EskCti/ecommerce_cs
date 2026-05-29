using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed record ResolveGradeVariantInputDto
{
    public Guid? GradeVariantId { get; init; }
    public IReadOnlyList<Guid>? OptionIds { get; init; }
}

public sealed record ResolveGradeVariantOutputDto
{
    public required Guid VariantId { get; init; }
    public required IReadOnlyList<Guid> OptionIds { get; init; }
    public required string Label { get; init; }
    public required int AvailableStock { get; init; }
}

public sealed class ResolveGradeVariantUseCase(IProductRepository productRepository)
    : IUseCase<(int tenantId, Guid productId, ResolveGradeVariantInputDto input), ResolveGradeVariantOutputDto>
{
    public async Task<Result<ResolveGradeVariantOutputDto>> Execute(
        (int tenantId, Guid productId, ResolveGradeVariantInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, productId, input) = request;

        var productResult = await productRepository.GetById(productId);
        if (productResult.IsFailure)
            return Result<ResolveGradeVariantOutputDto>.Failure(productResult.Error);

        var product = productResult.Value;
        if (product.TenantId.Value != tenantId)
            return Result<ResolveGradeVariantOutputDto>.Failure("Product does not belong to this tenant.");

        var variant = input.GradeVariantId is Guid variantId
            ? product.FindVariantById(variantId).Value
            : input.OptionIds is { Count: > 0 } optionIds
                ? product.FindVariantByOptions(optionIds).Value
                : null;

        if (variant is null)
            return Result<ResolveGradeVariantOutputDto>.Failure("Grade variant not found.");

        return Result<ResolveGradeVariantOutputDto>.Success(new ResolveGradeVariantOutputDto
        {
            VariantId = variant.Id,
            OptionIds = variant.OptionIds.ToList(),
            Label = variant.BuildLabel(product.GradeDimensions),
            AvailableStock = variant.Stock.Value
        });
    }
}
