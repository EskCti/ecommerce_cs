using RetailOps.Core.Catalog.Application.Events;
using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.Repositories;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Core.Sales.Application.Events;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Application.UseCases;

public sealed class SaleCompletedHandler(ICommissionRepository commissionRepository)
{
    public async Task<Result> Handle(SaleCompletedEvent @event)
    {
        var tenantResult = TenantId.Create(@event.TenantId);
        if (tenantResult.IsFailure)
            return Result.Failure(tenantResult.Error);

        var existingCommission = await commissionRepository.GetBySaleId(@event.SaleId);
        if (existingCommission.IsFailure)
            return Result.Failure(existingCommission.Error);

        if (existingCommission.Value is not null || @event.CommissionAmount <= 0)
            return Result.Success();

        var amountResult = Money.Create(@event.CommissionAmount);
        if (amountResult.IsFailure)
            return Result.Failure(amountResult.Error);

        var operatorLegacyId = LegacyUserIdHelper.TryParseLegacyUserId(@event.OperatorUserId);
        if (operatorLegacyId is null or <= 0)
            return Result.Failure("Operator legacy id is required for commission.");

        var commissionResult = Commission.Create(
            tenantResult.Value,
            @event.SaleId,
            operatorLegacyId.Value,
            amountResult.Value);

        if (commissionResult.IsFailure)
            return Result.Failure(commissionResult.Error);

        var saveCommission = await commissionRepository.Save(commissionResult.Value);
        return saveCommission.IsFailure ? Result.Failure(saveCommission.Error) : Result.Success();
    }
}

public sealed class ProductPurchasedHandler(IPayableRepository payableRepository)
{
    public async Task<Result> Handle(ProductPurchased @event)
    {
        var tenantResult = TenantId.Create(@event.TenantId);
        if (tenantResult.IsFailure)
            return Result.Failure(tenantResult.Error);

        var amountResult = Money.Create(@event.UnitCost * @event.Quantity);
        if (amountResult.IsFailure)
            return Result.Failure(amountResult.Error);

        var dueDateResult = DueDate.Create(@event.DueDate);
        if (dueDateResult.IsFailure)
            return Result.Failure(dueDateResult.Error);

        var description = FinanceEventDescriptionHelper.BuildPurchaseDescription(@event.ProductName, @event.Reason);
        var payableResult = Payable.CreatePurchase(
            tenantResult.Value,
            description,
            amountResult.Value,
            dueDateResult.Value,
            @event.ProductId,
            @event.SupplierLegacyId,
            @event.PurchasedAt);

        if (payableResult.IsFailure)
            return Result.Failure(payableResult.Error);

        var saveResult = await payableRepository.Save(payableResult.Value);
        return saveResult.IsFailure ? Result.Failure(saveResult.Error) : Result.Success();
    }
}

public sealed class FinanceCancellationService(
    IReceivableRepository receivableRepository,
    ICommissionRepository commissionRepository)
    : Application.Ports.IFinanceCancellationPort
{
    public async Task<Result> RemoveBySaleIdAsync(
        TenantId tenantId,
        Guid saleId,
        CancellationToken ct = default)
    {
        var receivableResult = await receivableRepository.GetBySaleId(saleId);
        if (receivableResult.IsFailure)
            return Result.Failure(receivableResult.Error);

        if (receivableResult.Value is not null && receivableResult.Value.TenantId.Value == tenantId.Value)
        {
            var deleteReceivable = await receivableRepository.Delete(receivableResult.Value.Id);
            if (deleteReceivable.IsFailure)
                return deleteReceivable;
        }

        return await commissionRepository.DeleteBySaleId(saleId);
    }
}

internal static class LegacyUserIdHelper
{
    internal static int? TryParseLegacyUserId(Guid userId)
    {
        var parts = userId.ToString().Split('-');
        if (parts.Length != 5 || parts[3] != "0000")
            return null;

        return int.TryParse(parts[4], out var legacyId) ? legacyId : null;
    }
}

internal static class FinanceEventDescriptionHelper
{
    internal static string BuildPurchaseDescription(string productName, string reason)
    {
        var name = string.IsNullOrWhiteSpace(productName) ? "Produto" : productName.Trim();
        var motive = string.IsNullOrWhiteSpace(reason) ? string.Empty : reason.Trim();
        var description = string.IsNullOrEmpty(motive) ? $"Compra — {name}" : $"Compra — {name} ({motive})";
        return description.Length <= 255 ? description : description[..255];
    }
}
