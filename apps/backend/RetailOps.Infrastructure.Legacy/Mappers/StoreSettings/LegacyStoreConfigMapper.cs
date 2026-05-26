using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;

public static class LegacyStoreConfigMapper
{
    public static Result<StoreConfig> ToDomain(LegacyStoreConfigRow row)
    {
        try
        {
            var tenantIdResult = TenantId.Create(row.CompanyId);
            if (tenantIdResult.IsFailure)
                return Result<StoreConfig>.Failure(tenantIdResult.Error);

            var storeNameResult = StoreName.Create(
                string.IsNullOrWhiteSpace(row.NomeSistema) ? "Minha Loja" : row.NomeSistema);
            if (storeNameResult.IsFailure)
                return Result<StoreConfig>.Failure(storeNameResult.Error);

            Cnpj? cnpj = null;
            if (!string.IsNullOrWhiteSpace(row.CnpjSistema))
            {
                var cnpjResult = Cnpj.Create(row.CnpjSistema);
                if (cnpjResult.IsFailure)
                    return Result<StoreConfig>.Failure(cnpjResult.Error);

                cnpj = cnpjResult.Value;
            }

            var discountType = DiscountTypeExtensions.FromLegacyValue(row.TipoDesconto ?? "N");
            var discountValue = 0m;

            var commissionRateResult = CommissionRate.Create(row.Comissao ?? 0);
            if (commissionRateResult.IsFailure)
                return Result<StoreConfig>.Failure(commissionRateResult.Error);

            var reportFormat = ReportFormat.FromLegacyValue(row.TipoRel ?? "P");

            ApiToken? apiToken = null;
            if (!string.IsNullOrWhiteSpace(row.Token))
            {
                var apiTokenResult = ApiToken.Create(row.Token);
                if (apiTokenResult.IsFailure)
                    return Result<StoreConfig>.Failure(apiTokenResult.Error);

                apiToken = apiTokenResult.Value;
            }

            ImagePath? logoPath = null;
            if (!string.IsNullOrWhiteSpace(row.FotoRel))
            {
                var logoPathResult = ImagePath.Create(row.FotoRel);
                if (logoPathResult.IsFailure)
                    return Result<StoreConfig>.Failure(logoPathResult.Error);

                logoPath = logoPathResult.Value;
            }

            return StoreConfig.Reconstitute(
                LegacyStoreSettingsIds.StoreConfig(row.CompanyId),
                tenantIdResult.Value,
                storeNameResult.Value,
                cnpj,
                discountType,
                discountValue,
                commissionRateResult.Value,
                reportFormat,
                apiToken,
                logoPath,
                row.Contatos,
                row.Endereco,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            return Result<StoreConfig>.Failure($"Failed to map legacy StoreConfig: {ex.Message}");
        }
    }

    public static Result<LegacyStoreConfigRow> ToLegacy(StoreConfig storeConfig)
    {
        try
        {
            var row = new LegacyStoreConfigRow
            {
                CompanyId = storeConfig.TenantId.Value,
                NomeSistema = storeConfig.Name?.Value,
                Contatos = storeConfig.Contacts,
                CnpjSistema = storeConfig.Cnpj?.Value,
                Endereco = storeConfig.Address,
                TipoRel = storeConfig.ReportFormat?.ToLegacyValue(),
                TipoDesconto = storeConfig.DiscountType.ToLegacyValue(),
                Comissao = storeConfig.CommissionRate?.Value,
                Token = storeConfig.ApiToken?.Value,
                FotoRel = storeConfig.LogoPath?.Value
            };

            return Result<LegacyStoreConfigRow>.Success(row);
        }
        catch (Exception ex)
        {
            return Result<LegacyStoreConfigRow>.Failure($"Failed to map StoreConfig to legacy: {ex.Message}");
        }
    }
}
