using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Core.Returns.Domain.Entities;
using RetailOps.Core.Returns.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Mappers.Catalog;
using RetailOps.Infrastructure.Legacy.Mappers.Crm;
using RetailOps.Infrastructure.Legacy.Mappers.Sales;
using RetailOps.Infrastructure.Legacy.Mappers.StoreSettings;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Returns;

public static class LegacyExchangeMapper
{
    public static Result<Exchange> ToDomain(
        LegacyExchangeRow row,
        LegacyGradeMovementDetailRow? gradeIn = null,
        LegacyGradeMovementDetailRow? gradeOut = null)
    {
        var tenantId = TenantId.Create(row.CompanyId);
        if (tenantId.IsFailure)
            return Result<Exchange>.Failure(tenantId.Error);

        var customerId = CustomerId.Create(LegacyCrmIds.Customer(row.CustomerLegacyId));
        if (customerId.IsFailure)
            return Result<Exchange>.Failure(customerId.Error);

        var quantity = ExchangeQuantity.One();
        if (quantity.IsFailure)
            return Result<Exchange>.Failure(quantity.Error);

        var gradeInSelection = ToGradeSelection(gradeIn);
        if (gradeInSelection.IsFailure)
            return Result<Exchange>.Failure(gradeInSelection.Error);

        var gradeOutSelection = ToGradeSelection(gradeOut);
        if (gradeOutSelection.IsFailure)
            return Result<Exchange>.Failure(gradeOutSelection.Error);

        var operatorUserId = LegacyStoreSettingsIds.User(row.UserLegacyId);

        return Exchange.Reconstitute(
            LegacyReturnsIds.Exchange(row.Id),
            tenantId.Value,
            customerId.Value,
            LegacyCatalogIds.Product(row.ProductInLegacyId),
            LegacyCatalogIds.Product(row.ProductOutLegacyId),
            gradeInSelection.Value,
            gradeOutSelection.Value,
            quantity.Value,
            operatorUserId,
            row.ExchangeDate);
    }

    public static Result<LegacyExchangeRow> ToLegacy(Exchange exchange, int? legacyId = null)
    {
        var customerLegacyId = LegacyCrmIds.ParseLegacyId(exchange.CustomerId.Value, "0006");
        var productInLegacyId = LegacyCatalogIds.ParseLegacyId(exchange.ProductInId, "0009");
        var productOutLegacyId = LegacyCatalogIds.ParseLegacyId(exchange.ProductOutId, "0009");
        var userLegacyId = LegacySalesIds.ParseOperatorLegacyId(exchange.OperatorUserId);

        if (customerLegacyId is null || productInLegacyId is null || productOutLegacyId is null || userLegacyId is null)
            return Result<LegacyExchangeRow>.Failure("Invalid exchange reference for legacy mapping.");

        return Result<LegacyExchangeRow>.Success(new LegacyExchangeRow
        {
            Id = legacyId ?? LegacyReturnsIds.ParseExchangeLegacyId(exchange.Id) ?? 0,
            CompanyId = exchange.TenantId.Value,
            CustomerLegacyId = customerLegacyId.Value,
            ProductInLegacyId = productInLegacyId.Value,
            ProductOutLegacyId = productOutLegacyId.Value,
            UserLegacyId = userLegacyId.Value,
            ExchangeDate = exchange.RegisteredAt
        });
    }

    public static Result<LegacyGradeMovementDetailRow> ToGradeDetailLegacy(
        TenantId tenantId,
        string movementType,
        int exchangeLegacyId,
        GradeSelection? grade,
        int quantity = 1)
    {
        if (grade is null || !grade.HasGrade)
            return Result<LegacyGradeMovementDetailRow>.Failure("Grade selection is required.");

        var primaryOption = grade.OptionIds.FirstOrDefault();
        if (primaryOption == Guid.Empty)
            return Result<LegacyGradeMovementDetailRow>.Failure("Grade option id is required.");

        var optionLegacyId = LegacyCatalogIds.ParseLegacyId(primaryOption, "0012");
        if (optionLegacyId is null)
            return Result<LegacyGradeMovementDetailRow>.Failure("Invalid grade option id.");

        var optionLegacyId2 = grade.OptionIds.Count > 1
            ? LegacyCatalogIds.ParseLegacyId(grade.OptionIds[1], "0012")
            : null;

        return Result<LegacyGradeMovementDetailRow>.Success(new LegacyGradeMovementDetailRow
        {
            CompanyId = tenantId.Value,
            MovementType = movementType,
            MovementLegacyId = exchangeLegacyId,
            OptionLegacyId = optionLegacyId.Value,
            OptionLegacyId2 = optionLegacyId2,
            Quantity = quantity
        });
    }

    private static Result<GradeSelection?> ToGradeSelection(LegacyGradeMovementDetailRow? row)
    {
        if (row is null)
            return Result<GradeSelection?>.Success(null);

        var optionIds = new List<Guid> { LegacyCatalogIds.GradeOption(row.OptionLegacyId) };
        if (row.OptionLegacyId2 is int second)
            optionIds.Add(LegacyCatalogIds.GradeOption(second));

        return GradeSelection.Create(null, optionIds);
    }
}
