using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class UpdateStoreConfigUseCase : IUseCase<(int tenantId, UpdateStoreConfigInputDto input), StoreConfigOutputDto>
{
    private readonly IStoreConfigRepository _storeConfigRepository;

    public UpdateStoreConfigUseCase(IStoreConfigRepository storeConfigRepository)
    {
        _storeConfigRepository = storeConfigRepository;
    }

    public async Task<Result<StoreConfigOutputDto>> Execute(
        (int tenantId, UpdateStoreConfigInputDto input) request, 
        CancellationToken cancellationToken = default)
    {
        var (tenantId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<StoreConfigOutputDto>.Failure(tenantIdResult.Error);

        var storeConfigResult = await _storeConfigRepository.GetByTenantId(tenantIdResult.Value);
        
        if (storeConfigResult.IsFailure)
        {
            // Create default store config if it doesn't exist
            var createResult = await CreateDefaultStoreConfig(tenantIdResult.Value, cancellationToken);
            if (createResult.IsFailure)
                return createResult;

            storeConfigResult = await _storeConfigRepository.GetByTenantId(tenantIdResult.Value);
        }

        var storeConfig = storeConfigResult.Value;

        // Update general info if provided
        if (input.Name != null || input.Cnpj != null || input.LogoPath != null
            || input.Contacts != null || input.Address != null)
        {
            var updateGeneralResult = UpdateGeneralInfo(storeConfig, input);
            if (updateGeneralResult.IsFailure)
                return Result<StoreConfigOutputDto>.Failure(updateGeneralResult.Error);
        }

        // Update discount settings if provided
        if (input.DiscountType != null || input.DiscountValue.HasValue)
        {
            var updateDiscountResult = UpdateDiscountSettings(storeConfig, input);
            if (updateDiscountResult.IsFailure)
                return Result<StoreConfigOutputDto>.Failure(updateDiscountResult.Error);
        }

        // Update report settings if provided
        if (input.CommissionRate.HasValue || input.ReportFormat != null)
        {
            var updateReportResult = UpdateReportSettings(storeConfig, input);
            if (updateReportResult.IsFailure)
                return Result<StoreConfigOutputDto>.Failure(updateReportResult.Error);
        }

        // Update integration token if provided
        if (input.ApiToken != null)
        {
            var updateTokenResult = UpdateIntegrationToken(storeConfig, input);
            if (updateTokenResult.IsFailure)
                return Result<StoreConfigOutputDto>.Failure(updateTokenResult.Error);
        }

        var saveResult = await _storeConfigRepository.Save(storeConfig);
        if (saveResult.IsFailure)
            return Result<StoreConfigOutputDto>.Failure(saveResult.Error);

        var outputDto = StoreConfigOutputDto.FromDomain(storeConfig);
        return Result<StoreConfigOutputDto>.Success(outputDto);
    }

    private Result UpdateGeneralInfo(StoreConfig storeConfig, UpdateStoreConfigInputDto input)
    {
        StoreName? storeName = null;
        if (input.Name != null)
        {
            var storeNameResult = StoreName.Create(input.Name);
            if (storeNameResult.IsFailure)
                return storeNameResult;

            storeName = storeNameResult.Value;
        }

        Cnpj? cnpj = null;
        if (input.Cnpj != null)
        {
            var cnpjResult = Cnpj.Create(input.Cnpj);
            if (cnpjResult.IsFailure)
                return cnpjResult;

            cnpj = cnpjResult.Value;
        }

        ImagePath? logoPath = null;
        if (input.LogoPath != null)
        {
            var logoPathResult = ImagePath.Create(input.LogoPath);
            if (logoPathResult.IsFailure)
                return logoPathResult;

            logoPath = logoPathResult.Value;
        }

        return storeConfig.UpdateGeneralInfo(
            storeName ?? storeConfig.Name,
            cnpj,
            logoPath,
            input.Contacts ?? storeConfig.Contacts,
            input.Address ?? storeConfig.Address);
    }

    private Result UpdateDiscountSettings(StoreConfig storeConfig, UpdateStoreConfigInputDto input)
    {
        var discountType = storeConfig.DiscountType;
        if (input.DiscountType != null)
        {
            if (!Enum.TryParse<DiscountType>(input.DiscountType, true, out var parsedDiscountType))
                return Result.Failure($"Invalid discount type: {input.DiscountType}");

            discountType = parsedDiscountType;
        }

        var discountValue = input.DiscountValue ?? storeConfig.DiscountValue;

        return storeConfig.UpdateDiscountSettings(discountType, discountValue);
    }

    private Result UpdateReportSettings(StoreConfig storeConfig, UpdateStoreConfigInputDto input)
    {
        var commissionRate = storeConfig.CommissionRate;
        if (input.CommissionRate.HasValue)
        {
            var commissionRateResult = CommissionRate.Create(input.CommissionRate.Value);
            if (commissionRateResult.IsFailure)
                return commissionRateResult;

            commissionRate = commissionRateResult.Value;
        }

        var reportFormat = storeConfig.ReportFormat;
        if (input.ReportFormat != null)
        {
            var reportFormatResult = ReportFormat.Create(input.ReportFormat);
            if (reportFormatResult.IsFailure)
                return reportFormatResult;

            reportFormat = reportFormatResult.Value;
        }

        return storeConfig.UpdateReportSettings(commissionRate, reportFormat);
    }

    private Result UpdateIntegrationToken(StoreConfig storeConfig, UpdateStoreConfigInputDto input)
    {
        ApiToken? apiToken = null;
        if (input.ApiToken != null)
        {
            var apiTokenResult = ApiToken.Create(input.ApiToken);
            if (apiTokenResult.IsFailure)
                return apiTokenResult;

            apiToken = apiTokenResult.Value;
        }

        return storeConfig.UpdateIntegrationToken(apiToken);
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