using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Catalog.Application.Ports;
using RetailOps.Core.Catalog.Domain.Entities;
using RetailOps.Core.Catalog.Domain.Services;
using RetailOps.Infrastructure.Legacy.Mappers.Catalog;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy;

public sealed class CatalogLegacyAdapter(
    LegacySasDbContext db,
    GradeVariantSyncService gradeVariantSync) : ICatalogLegacyPort
{
    public async Task<Result<IReadOnlyList<Product>>> GetProductsFromLegacyAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        try
        {
            var rows = await db.Products.AsNoTracking()
                .Where(r => r.CompanyId == tenantId.Value)
                .OrderBy(r => r.Name)
                .ToListAsync(ct);

            var products = new List<Product>();
            foreach (var row in rows)
            {
                var productId = LegacyCatalogIds.Product(row.Id);
                var grades = await LoadGradeDimensionsAsync(tenantId, row.Id, productId, ct);
                var mapped = LegacyProductMapper.ToDomain(row, grades);
                if (mapped.IsSuccess)
                {
                    await ApplyGradeVariantsAsync(tenantId, row.Id, mapped.Value, ct);
                    products.Add(mapped.Value);
                }
            }

            return Result<IReadOnlyList<Product>>.Success(products);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Product>>.Failure($"Failed to get Products from legacy: {ex.Message}");
        }
    }

    public async Task<Result<Product?>> GetProductFromLegacyAsync(
        TenantId tenantId,
        Guid productId,
        CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCatalogIds.ParseLegacyId(productId, "0009");
            if (legacyId is null)
                return Result<Product?>.Success(null);

            var row = await db.Products.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result<Product?>.Success(null);

            var grades = await LoadGradeDimensionsAsync(tenantId, row.Id, productId, ct);
            var result = LegacyProductMapper.ToDomain(row, grades);
            if (result.IsFailure)
                return Result<Product?>.Failure(result.Error);

            await ApplyGradeVariantsAsync(tenantId, row.Id, result.Value, ct);
            return Result<Product?>.Success(result.Value);
        }
        catch (Exception ex)
        {
            return Result<Product?>.Failure($"Failed to get Product from legacy: {ex.Message}");
        }
    }

    public async Task<Result<Product?>> GetProductByBarcodeFromLegacyAsync(
        TenantId tenantId,
        string barcode,
        CancellationToken ct = default)
    {
        try
        {
            var row = await db.Products.AsNoTracking()
                .FirstOrDefaultAsync(r => r.CompanyId == tenantId.Value && r.Code == barcode, ct);

            if (row is null)
                return Result<Product?>.Success(null);

            var productId = LegacyCatalogIds.Product(row.Id);
            var grades = await LoadGradeDimensionsAsync(tenantId, row.Id, productId, ct);
            var result = LegacyProductMapper.ToDomain(row, grades);
            if (result.IsFailure)
                return Result<Product?>.Failure(result.Error);

            await ApplyGradeVariantsAsync(tenantId, row.Id, result.Value, ct);
            return Result<Product?>.Success(result.Value);
        }
        catch (Exception ex)
        {
            return Result<Product?>.Failure($"Failed to get Product by barcode from legacy: {ex.Message}");
        }
    }

    public async Task<Result<int>> SaveProductToLegacyAsync(Product product, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCatalogIds.ParseLegacyId(product.Id, "0009");
            var rowResult = LegacyProductMapper.ToLegacy(product, legacyId);
            if (rowResult.IsFailure)
                return Result<int>.Failure(rowResult.Error);

            var row = rowResult.Value;
            int savedLegacyId;

            if (legacyId is > 0)
            {
                var existing = await db.Products
                    .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == product.TenantId.Value, ct);

                if (existing is not null)
                {
                    MapProductRow(existing, row);
                    await db.SaveChangesAsync(ct);
                    savedLegacyId = existing.Id;
                }
                else
                {
                    db.Products.Add(row);
                    await db.SaveChangesAsync(ct);
                    savedLegacyId = row.Id;
                }
            }
            else
            {
                db.Products.Add(row);
                await db.SaveChangesAsync(ct);
                savedLegacyId = row.Id;
            }

            product.SyncIdentity(LegacyCatalogIds.Product(savedLegacyId));
            await SaveGradeDimensionsAsync(product, savedLegacyId, ct);
            await PruneOrphanGradeDataAsync(product, savedLegacyId, ct);
            await SaveGradeVariantsAsync(product, savedLegacyId, ct);

            return Result<int>.Success(savedLegacyId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save Product to legacy: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<Category>>> GetCategoriesFromLegacyAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        try
        {
            var rows = await db.Categories.AsNoTracking()
                .Where(r => r.CompanyId == tenantId.Value)
                .OrderBy(r => r.Name)
                .ToListAsync(ct);

            var categories = new List<Category>();
            foreach (var row in rows)
            {
                var mapped = LegacyCategoryMapper.ToDomain(row);
                if (mapped.IsSuccess)
                    categories.Add(mapped.Value);
            }

            return Result<IReadOnlyList<Category>>.Success(categories);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Category>>.Failure($"Failed to get Categories from legacy: {ex.Message}");
        }
    }

    public async Task<Result<Category?>> GetCategoryFromLegacyAsync(
        TenantId tenantId,
        Guid categoryId,
        CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCatalogIds.ParseLegacyId(categoryId, "0010");
            if (legacyId is null)
                return Result<Category?>.Success(null);

            var row = await db.Categories.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result<Category?>.Success(null);

            var result = LegacyCategoryMapper.ToDomain(row);
            return result.IsFailure
                ? Result<Category?>.Failure(result.Error)
                : Result<Category?>.Success(result.Value);
        }
        catch (Exception ex)
        {
            return Result<Category?>.Failure($"Failed to get Category from legacy: {ex.Message}");
        }
    }

    public async Task<Result<int>> SaveCategoryToLegacyAsync(Category category, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCatalogIds.ParseLegacyId(category.Id, "0010");
            var rowResult = LegacyCategoryMapper.ToLegacy(category, legacyId);
            if (rowResult.IsFailure)
                return Result<int>.Failure(rowResult.Error);

            var row = rowResult.Value;

            if (legacyId is > 0)
            {
                var existing = await db.Categories
                    .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == category.TenantId.Value, ct);

                if (existing is not null)
                {
                    existing.Name = row.Name;
                    existing.Active = row.Active;
                    await db.SaveChangesAsync(ct);
                    category.SyncIdentity(LegacyCatalogIds.Category(existing.Id));
                    return Result<int>.Success(existing.Id);
                }
            }

            db.Categories.Add(row);
            await db.SaveChangesAsync(ct);
            category.SyncIdentity(LegacyCatalogIds.Category(row.Id));
            return Result<int>.Success(row.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save Category to legacy: {ex.Message}");
        }
    }

    public async Task<Result<int>> SaveStockMovementToLegacyAsync(
        StockMovement movement,
        TenantId tenantId,
        CancellationToken ct = default)
    {
        try
        {
            var productLegacyId = LegacyCatalogIds.ParseLegacyId(movement.ProductId, "0009");
            if (productLegacyId is null)
                return Result<int>.Failure("Invalid product id for stock movement.");

            if (movement.Type is StockMovementType.Exit)
            {
                var legacyId = LegacyCatalogIds.ParseLegacyId(movement.Id, "0014");
                var rowResult = LegacyStockMovementMapper.ToExitLegacy(
                    movement,
                    productLegacyId.Value,
                    tenantId.Value,
                    legacyId);

                if (rowResult.IsFailure)
                    return Result<int>.Failure(rowResult.Error);

                var row = rowResult.Value;

                if (legacyId is > 0)
                {
                    var existing = await db.StockExits.FirstOrDefaultAsync(e => e.Id == legacyId, ct);
                    if (existing is not null)
                    {
                        existing.Quantity = row.Quantity;
                        existing.Reason = row.Reason;
                        await db.SaveChangesAsync(ct);
                        movement.SyncIdentity(LegacyCatalogIds.StockExit(existing.Id));
                        return Result<int>.Success(existing.Id);
                    }
                }

                db.StockExits.Add(row);
                await db.SaveChangesAsync(ct);
                movement.SyncIdentity(LegacyCatalogIds.StockExit(row.Id));
                return Result<int>.Success(row.Id);
            }

            var entryLegacyId = LegacyCatalogIds.ParseLegacyId(movement.Id, "0013");
            var entryResult = LegacyStockMovementMapper.ToEntryLegacy(
                movement,
                productLegacyId.Value,
                tenantId.Value,
                entryLegacyId);

            if (entryResult.IsFailure)
                return Result<int>.Failure(entryResult.Error);

            var entryRow = entryResult.Value;
            entryRow.MovementType = movement.Type.ToString();

            if (entryLegacyId is > 0)
            {
                var existingEntry = await db.StockEntries.FirstOrDefaultAsync(e => e.Id == entryLegacyId, ct);
                if (existingEntry is not null)
                {
                    existingEntry.Quantity = entryRow.Quantity;
                    existingEntry.Reason = entryRow.Reason;
                    existingEntry.MovementType = entryRow.MovementType;
                    await db.SaveChangesAsync(ct);
                    movement.SyncIdentity(LegacyCatalogIds.StockEntry(existingEntry.Id));
                    return Result<int>.Success(existingEntry.Id);
                }
            }

            db.StockEntries.Add(entryRow);
            await db.SaveChangesAsync(ct);
            movement.SyncIdentity(LegacyCatalogIds.StockEntry(entryRow.Id));
            return Result<int>.Success(entryRow.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save Stock movement to legacy: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<StockMovement>>> GetStockMovementsFromLegacyAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        try
        {
            var productLegacyId = LegacyCatalogIds.ParseLegacyId(productId, "0009");
            if (productLegacyId is null)
                return Result<IReadOnlyList<StockMovement>>.Success([]);

            var movements = new List<StockMovement>();

            var entries = await db.StockEntries.AsNoTracking()
                .Where(e => e.ProductLegacyId == productLegacyId)
                .ToListAsync(ct);

            foreach (var entry in entries)
            {
                var mapped = LegacyStockMovementMapper.ToDomain(entry, productId);
                if (mapped.IsSuccess)
                    movements.Add(mapped.Value);
            }

            var exits = await db.StockExits.AsNoTracking()
                .Where(e => e.ProductLegacyId == productLegacyId)
                .ToListAsync(ct);

            foreach (var exit in exits)
            {
                var mapped = LegacyStockMovementMapper.ToDomain(exit, productId);
                if (mapped.IsSuccess)
                    movements.Add(mapped.Value);
            }

            return Result<IReadOnlyList<StockMovement>>.Success(
                movements.OrderByDescending(m => m.CreatedAt).ToList());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<StockMovement>>.Failure(
                $"Failed to get Stock movements from legacy: {ex.Message}");
        }
    }

    private async Task<IReadOnlyList<GradeDimension>> LoadGradeDimensionsAsync(
        TenantId tenantId,
        int productLegacyId,
        Guid productId,
        CancellationToken ct)
    {
        var dimensionRows = await db.GradeDimensions.AsNoTracking()
            .Where(d => d.CompanyId == tenantId.Value && d.ProductLegacyId == productLegacyId)
            .ToListAsync(ct);

        var dimensions = new List<GradeDimension>();
        foreach (var dimensionRow in dimensionRows)
        {
            var optionRows = await db.GradeOptions.AsNoTracking()
                .Where(o => o.CompanyId == tenantId.Value && o.DimensionLegacyId == dimensionRow.Id)
                .ToListAsync(ct);

            var options = new List<GradeOption>();
            foreach (var optionRow in optionRows)
            {
                var optionMapped = LegacyGradeDetailMapper.ToOptionDomain(optionRow);
                if (optionMapped.IsSuccess)
                    options.Add(optionMapped.Value);
            }

            var dimensionMapped = LegacyGradeDetailMapper.ToDimensionDomain(dimensionRow, options);
            if (dimensionMapped.IsSuccess)
                dimensions.Add(dimensionMapped.Value);
        }

        return dimensions;
    }

    private async Task ApplyGradeVariantsAsync(
        TenantId tenantId,
        int productLegacyId,
        Product product,
        CancellationToken ct)
    {
        var variantRows = await db.GradeVariants.AsNoTracking()
            .Where(v => v.CompanyId == tenantId.Value && v.ProductLegacyId == productLegacyId)
            .ToListAsync(ct);

        var variants = new List<GradeVariant>();
        foreach (var row in variantRows)
        {
            var mapped = LegacyGradeVariantMapper.ToDomain(row, product.Id, product.GradeDimensions);
            if (mapped.IsSuccess)
                variants.Add(mapped.Value);
        }

        if (variants.Count > 0)
            product.SetGradeVariants(variants);
        else
            gradeVariantSync.MigrateOptionsStockToVariants(product);
    }

    private async Task SaveGradeVariantsAsync(Product product, int productLegacyId, CancellationToken ct)
    {
        var existingRows = await db.GradeVariants
            .Where(v => v.CompanyId == product.TenantId.Value && v.ProductLegacyId == productLegacyId)
            .ToListAsync(ct);

        db.GradeVariants.RemoveRange(existingRows);

        foreach (var variant in product.GradeVariants)
        {
            var rowResult = LegacyGradeVariantMapper.ToLegacy(
                variant,
                productLegacyId,
                product.GradeDimensions);

            if (rowResult.IsFailure)
                continue;

            var row = rowResult.Value;
            row.CompanyId = product.TenantId.Value;
            db.GradeVariants.Add(row);
            await db.SaveChangesAsync(ct);
            variant.SyncIdentity(LegacyCatalogIds.GradeVariant(row.Id));
        }
    }

    private async Task SaveGradeDimensionsAsync(Product product, int productLegacyId, CancellationToken ct)
    {
        foreach (var dimension in product.GradeDimensions)
        {
            var dimensionLegacyId = LegacyCatalogIds.ParseLegacyId(dimension.Id, "0011");
            var dimensionRowResult = LegacyGradeDetailMapper.ToDimensionLegacy(
                dimension,
                productLegacyId,
                dimensionLegacyId);

            if (dimensionRowResult.IsFailure)
                continue;

            var dimensionRow = dimensionRowResult.Value;
            dimensionRow.CompanyId = product.TenantId.Value;

            int savedDimensionId;
            if (dimensionLegacyId is > 0)
            {
                var existing = await db.GradeDimensions
                    .FirstOrDefaultAsync(d => d.Id == dimensionLegacyId, ct);

                if (existing is not null)
                {
                    existing.Name = dimensionRow.Name;
                    existing.ProductLegacyId = productLegacyId;
                    await db.SaveChangesAsync(ct);
                    savedDimensionId = existing.Id;
                }
                else
                {
                    db.GradeDimensions.Add(dimensionRow);
                    await db.SaveChangesAsync(ct);
                    savedDimensionId = dimensionRow.Id;
                }
            }
            else
            {
                db.GradeDimensions.Add(dimensionRow);
                await db.SaveChangesAsync(ct);
                savedDimensionId = dimensionRow.Id;
            }

            dimension.SyncIdentity(LegacyCatalogIds.GradeDimension(savedDimensionId));

            foreach (var option in dimension.Options)
            {
                var optionLegacyId = LegacyCatalogIds.ParseLegacyId(option.Id, "0012");
                var optionRowResult = LegacyGradeDetailMapper.ToOptionLegacy(
                    option,
                    savedDimensionId,
                    optionLegacyId);

                if (optionRowResult.IsFailure)
                    continue;

                var optionRow = optionRowResult.Value;
                optionRow.CompanyId = product.TenantId.Value;

                if (optionLegacyId is > 0)
                {
                    var existingOption = await db.GradeOptions
                        .FirstOrDefaultAsync(o => o.Id == optionLegacyId, ct);

                    if (existingOption is not null)
                    {
                        existingOption.Label = optionRow.Label;
                        existingOption.Stock = optionRow.Stock;
                        await db.SaveChangesAsync(ct);
                        option.SyncIdentity(LegacyCatalogIds.GradeOption(existingOption.Id));
                        continue;
                    }
                }

                db.GradeOptions.Add(optionRow);
                await db.SaveChangesAsync(ct);
                option.SyncIdentity(LegacyCatalogIds.GradeOption(optionRow.Id));
            }
        }
    }

    private async Task PruneOrphanGradeDataAsync(Product product, int productLegacyId, CancellationToken ct)
    {
        var dimensionRows = await db.GradeDimensions
            .Where(d => d.CompanyId == product.TenantId.Value && d.ProductLegacyId == productLegacyId)
            .ToListAsync(ct);

        var currentDimensionLegacyIds = product.GradeDimensions
            .Select(d => LegacyCatalogIds.ParseLegacyId(d.Id, "0011"))
            .Where(id => id is > 0)
            .Select(id => id!.Value)
            .ToHashSet();

        var currentOptionLegacyIds = product.GradeDimensions
            .SelectMany(d => d.Options)
            .Select(o => LegacyCatalogIds.ParseLegacyId(o.Id, "0012"))
            .Where(id => id is > 0)
            .Select(id => id!.Value)
            .ToHashSet();

        foreach (var dimensionRow in dimensionRows)
        {
            var optionRows = await db.GradeOptions
                .Where(o => o.CompanyId == product.TenantId.Value && o.DimensionLegacyId == dimensionRow.Id)
                .ToListAsync(ct);

            if (!currentDimensionLegacyIds.Contains(dimensionRow.Id))
            {
                db.GradeOptions.RemoveRange(optionRows);
                db.GradeDimensions.Remove(dimensionRow);
                continue;
            }

            var orphanOptions = optionRows.Where(o => !currentOptionLegacyIds.Contains(o.Id)).ToList();
            if (orphanOptions.Count > 0)
                db.GradeOptions.RemoveRange(orphanOptions);
        }

        await db.SaveChangesAsync(ct);
    }

    private static void MapProductRow(LegacyProductRow existing, LegacyProductRow row)
    {
        existing.Code = row.Code;
        existing.Name = row.Name;
        existing.Description = row.Description;
        existing.Stock = row.Stock;
        existing.SalePrice = row.SalePrice;
        existing.CostPrice = row.CostPrice;
        existing.ProfitMargin = row.ProfitMargin;
        existing.SupplierLegacyId = row.SupplierLegacyId;
        existing.CategoryLegacyId = row.CategoryLegacyId;
        existing.StockAlertLevel = row.StockAlertLevel;
        existing.Active = row.Active;
        existing.Photo = row.Photo;
    }
}

public sealed class StockLegacyAdapter(LegacySasDbContext db) : IStockLegacyPort
{
    public async Task<Result> AdjustStockAsync(
        TenantId tenantId,
        Guid productId,
        int newQuantity,
        CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCatalogIds.ParseLegacyId(productId, "0009");
            if (legacyId is null)
                return Result.Failure("Invalid product id.");

            var product = await db.Products
                .FirstOrDefaultAsync(p => p.Id == legacyId && p.CompanyId == tenantId.Value, ct);

            if (product is null)
                return Result.Failure("Product not found.");

            product.Stock = newQuantity;
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to adjust stock: {ex.Message}");
        }
    }

    public async Task<Result> ReserveStockAsync(
        TenantId tenantId,
        Guid productId,
        int quantity,
        CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCatalogIds.ParseLegacyId(productId, "0009");
            if (legacyId is null)
                return Result.Failure("Invalid product id.");

            var product = await db.Products
                .FirstOrDefaultAsync(p => p.Id == legacyId && p.CompanyId == tenantId.Value, ct);

            if (product is null)
                return Result.Failure("Product not found.");

            if (product.Stock < quantity)
                return Result.Failure("Insufficient stock.");

            product.Stock -= quantity;
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to reserve stock: {ex.Message}");
        }
    }

    public async Task<Result> ReleaseStockAsync(
        TenantId tenantId,
        Guid productId,
        int quantity,
        CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCatalogIds.ParseLegacyId(productId, "0009");
            if (legacyId is null)
                return Result.Failure("Invalid product id.");

            var product = await db.Products
                .FirstOrDefaultAsync(p => p.Id == legacyId && p.CompanyId == tenantId.Value, ct);

            if (product is null)
                return Result.Failure("Product not found.");

            product.Stock += quantity;
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to release stock: {ex.Message}");
        }
    }

    public async Task<Result> ReserveVariantStockAsync(
        TenantId tenantId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default) =>
        await AdjustVariantStockDeltaAsync(tenantId, variantId, -quantity, ct);

    public async Task<Result> ReleaseVariantStockAsync(
        TenantId tenantId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default) =>
        await AdjustVariantStockDeltaAsync(tenantId, variantId, quantity, ct);

    public async Task<Result> AdjustVariantStockAsync(
        TenantId tenantId,
        Guid variantId,
        int newQuantity,
        CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCatalogIds.ParseLegacyId(variantId, "0015");
            if (legacyId is null)
                return Result.Failure("Invalid grade variant id.");

            var row = await db.GradeVariants
                .FirstOrDefaultAsync(v => v.Id == legacyId && v.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result.Failure("Grade variant not found.");

            row.Stock = newQuantity;
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to adjust variant stock: {ex.Message}");
        }
    }

    private async Task<Result> AdjustVariantStockDeltaAsync(
        TenantId tenantId,
        Guid variantId,
        int delta,
        CancellationToken ct)
    {
        try
        {
            var legacyId = LegacyCatalogIds.ParseLegacyId(variantId, "0015");
            if (legacyId is null)
                return Result.Failure("Invalid grade variant id.");

            var row = await db.GradeVariants
                .FirstOrDefaultAsync(v => v.Id == legacyId && v.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result.Failure("Grade variant not found.");

            if (delta < 0 && row.Stock < Math.Abs(delta))
                return Result.Failure("Insufficient variant stock.");

            row.Stock += delta;
            await db.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update variant stock: {ex.Message}");
        }
    }
}
