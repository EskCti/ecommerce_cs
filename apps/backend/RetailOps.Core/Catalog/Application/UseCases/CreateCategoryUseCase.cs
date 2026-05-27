using RetailOps.Core.Catalog.Application.DTOs;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Repositories;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Application.UseCases;

public sealed class CreateCategoryUseCase(ICategoryRepository categoryRepository)
    : IUseCase<(int tenantId, CreateCategoryInputDto input), CategoryOutputDto>
{
    public async Task<Result<CategoryOutputDto>> Execute(
        (int tenantId, CreateCategoryInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CategoryOutputDto>.Failure(tenantIdResult.Error);

        var nameResult = ProductName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<CategoryOutputDto>.Failure(nameResult.Error);

        var categoryResult = Category.Create(tenantIdResult.Value, nameResult.Value, input.IsActive);
        if (categoryResult.IsFailure)
            return Result<CategoryOutputDto>.Failure(categoryResult.Error);

        var saveResult = await categoryRepository.Save(categoryResult.Value);
        if (saveResult.IsFailure)
            return Result<CategoryOutputDto>.Failure(saveResult.Error);

        return Result<CategoryOutputDto>.Success(CategoryOutputDto.FromDomain(categoryResult.Value));
    }
}

public sealed class UpdateCategoryUseCase(ICategoryRepository categoryRepository)
    : IUseCase<(int tenantId, Guid categoryId, UpdateCategoryInputDto input), CategoryOutputDto>
{
    public async Task<Result<CategoryOutputDto>> Execute(
        (int tenantId, Guid categoryId, UpdateCategoryInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, categoryId, input) = request;

        var categoryResult = await categoryRepository.GetById(categoryId);
        if (categoryResult.IsFailure)
            return Result<CategoryOutputDto>.Failure(categoryResult.Error);

        var category = categoryResult.Value;
        if (category.TenantId.Value != tenantId)
            return Result<CategoryOutputDto>.Failure("Category does not belong to this tenant.");

        if (input.Name is not null)
        {
            var nameResult = ProductName.Create(input.Name);
            if (nameResult.IsFailure)
                return Result<CategoryOutputDto>.Failure(nameResult.Error);

            var updateResult = category.UpdateName(nameResult.Value);
            if (updateResult.IsFailure)
                return Result<CategoryOutputDto>.Failure(updateResult.Error);
        }

        if (input.IsActive.HasValue)
        {
            var statusResult = input.IsActive.Value ? category.Activate() : category.Deactivate();
            if (statusResult.IsFailure)
                return Result<CategoryOutputDto>.Failure(statusResult.Error);
        }

        var saveResult = await categoryRepository.Save(category);
        if (saveResult.IsFailure)
            return Result<CategoryOutputDto>.Failure(saveResult.Error);

        return Result<CategoryOutputDto>.Success(CategoryOutputDto.FromDomain(category));
    }
}

public sealed class GetCategoryUseCase(ICategoryRepository categoryRepository)
    : IUseCase<(int tenantId, Guid categoryId), CategoryOutputDto>
{
    public async Task<Result<CategoryOutputDto>> Execute(
        (int tenantId, Guid categoryId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, categoryId) = request;

        var categoryResult = await categoryRepository.GetById(categoryId);
        if (categoryResult.IsFailure)
            return Result<CategoryOutputDto>.Failure(categoryResult.Error);

        if (categoryResult.Value.TenantId.Value != tenantId)
            return Result<CategoryOutputDto>.Failure("Category does not belong to this tenant.");

        return Result<CategoryOutputDto>.Success(CategoryOutputDto.FromDomain(categoryResult.Value));
    }
}
