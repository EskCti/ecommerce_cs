using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Catalog;

public static class LegacyCategoryMapper
{
    public static Result<Category> ToDomain(LegacyCategoryRow row)
    {
        try
        {
            var tenantIdResult = TenantId.Create(row.CompanyId);
            if (tenantIdResult.IsFailure)
                return Result<Category>.Failure(tenantIdResult.Error);

            if (string.IsNullOrWhiteSpace(row.Name))
                return Result<Category>.Failure("Category name is required.");

            var nameResult = ProductName.Create(row.Name);
            if (nameResult.IsFailure)
                return Result<Category>.Failure(nameResult.Error);

            var isActive = row.Active is null
                || row.Active.Equals("S", StringComparison.OrdinalIgnoreCase)
                || row.Active.Equals("Sim", StringComparison.OrdinalIgnoreCase);

            return Category.Reconstitute(
                LegacyCatalogIds.Category(row.Id),
                tenantIdResult.Value,
                nameResult.Value,
                isActive,
                DateTime.UtcNow,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            return Result<Category>.Failure($"Failed to map legacy Category: {ex.Message}");
        }
    }

    public static Result<LegacyCategoryRow> ToLegacy(Category category, int? legacyId = null)
    {
        try
        {
            var row = new LegacyCategoryRow
            {
                Id = legacyId ?? LegacyCatalogIds.ParseLegacyId(category.Id, "0010") ?? 0,
                CompanyId = category.TenantId.Value,
                Name = category.Name.Value,
                Active = category.IsActive ? "Sim" : "Não"
            };

            return Result<LegacyCategoryRow>.Success(row);
        }
        catch (Exception ex)
        {
            return Result<LegacyCategoryRow>.Failure($"Failed to map Category to legacy: {ex.Message}");
        }
    }
}
