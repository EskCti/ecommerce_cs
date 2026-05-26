using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class GetStoreConfigUseCase : IUseCase<int, StoreConfigOutputDto>
{
    private readonly IStoreConfigRepository _storeConfigRepository;

    public GetStoreConfigUseCase(IStoreConfigRepository storeConfigRepository)
    {
        _storeConfigRepository = storeConfigRepository;
    }

    public async Task<Result<StoreConfigOutputDto>> Execute(int tenantId, CancellationToken cancellationToken = default)
    {
        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<StoreConfigOutputDto>.Failure(tenantIdResult.Error);

        var storeConfigResult = await _storeConfigRepository.GetByTenantId(tenantIdResult.Value);
        
        if (storeConfigResult.IsFailure)
        {
            // Create default store config if it doesn't exist (create-on-first-read)
            return await CreateDefaultStoreConfig(tenantIdResult.Value, cancellationToken);
        }

        var storeConfig = storeConfigResult.Value;
        var outputDto = StoreConfigOutputDto.FromDomain(storeConfig);
        
        return Result<StoreConfigOutputDto>.Success(outputDto);
    }

    private async Task<Result<StoreConfigOutputDto>> CreateDefaultStoreConfig(TenantId tenantId, CancellationToken cancellationToken)
    {
        var storeNameResult = StoreName.Create("Minha Loja");
        if (storeNameResult.IsFailure)
            return Result<StoreConfigOutputDto>.Failure(storeNameResult.Error);

        var commissionRateResult = CommissionRate.Create(0);
        if (commissionRateResult.IsFailure)
            return Result<StoreConfigOutputDto>.Failure(commissionRateResult.Error);

        var reportFormatResult = ReportFormat.Create("PDF");
        if (reportFormatResult.IsFailure)
            return Result<StoreConfigOutputDto>.Failure(reportFormatResult.Error);

        var storeConfigResult = StoreConfig.Create(
            tenantId,
            storeNameResult.Value,
            cnpj: null,
            DiscountType.None,
            discountValue: 0,
            commissionRateResult.Value,
            reportFormatResult.Value);

        if (storeConfigResult.IsFailure)
            return Result<StoreConfigOutputDto>.Failure(storeConfigResult.Error);

        var storeConfig = storeConfigResult.Value;
        var saveResult = await _storeConfigRepository.Save(storeConfig);
        
        if (saveResult.IsFailure)
            return Result<StoreConfigOutputDto>.Failure(saveResult.Error);

        var outputDto = StoreConfigOutputDto.FromDomain(storeConfig);
        return Result<StoreConfigOutputDto>.Success(outputDto);
    }
}