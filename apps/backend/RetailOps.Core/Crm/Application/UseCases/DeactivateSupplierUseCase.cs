using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.UseCases;

public sealed class DeactivateSupplierUseCase(ISupplierRepository supplierRepository)
    : IUseCase<(int tenantId, Guid supplierId), SupplierOutputDto>
{
    public async Task<Result<SupplierOutputDto>> Execute(
        (int tenantId, Guid supplierId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, supplierId) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(tenantIdResult.Error);

        var supplierResult = await supplierRepository.GetById(supplierId);
        if (supplierResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(supplierResult.Error);

        var supplier = supplierResult.Value;
        if (supplier.TenantId.Value != tenantId)
            return Result<SupplierOutputDto>.Failure("Supplier does not belong to this tenant.");

        var deactivateResult = supplier.Deactivate();
        if (deactivateResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(deactivateResult.Error);

        var saveResult = await supplierRepository.Save(supplier);
        if (saveResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(saveResult.Error);

        return Result<SupplierOutputDto>.Success(SupplierOutputDto.FromDomain(supplier));
    }
}
