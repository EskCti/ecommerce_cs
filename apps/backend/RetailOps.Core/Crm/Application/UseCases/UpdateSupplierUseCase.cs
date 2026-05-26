using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.UseCases;

public sealed class UpdateSupplierUseCase(ISupplierRepository supplierRepository)
    : IUseCase<(int tenantId, Guid supplierId, UpdateSupplierInputDto input), SupplierOutputDto>
{
    public async Task<Result<SupplierOutputDto>> Execute(
        (int tenantId, Guid supplierId, UpdateSupplierInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, supplierId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(tenantIdResult.Error);

        var supplierResult = await supplierRepository.GetById(supplierId);
        if (supplierResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(supplierResult.Error);

        var supplier = supplierResult.Value;
        if (supplier.TenantId.Value != tenantId)
            return Result<SupplierOutputDto>.Failure("Supplier does not belong to this tenant.");

        PersonName? name = null;
        if (input.Name is not null)
        {
            var nameResult = PersonName.Create(input.Name);
            if (nameResult.IsFailure)
                return Result<SupplierOutputDto>.Failure(nameResult.Error);
            name = nameResult.Value;
        }

        PersonType? personType = input.PersonType;
        TaxDocument? taxDocument = null;
        if (input.PersonType.HasValue || input.TaxDocument is not null)
        {
            var resolvedPersonType = input.PersonType ?? supplier.PersonType;
            if (input.TaxDocument is null)
                return Result<SupplierOutputDto>.Failure("Tax document is required when updating person type.");

            var taxDocumentResult = TaxDocument.Create(input.TaxDocument, resolvedPersonType);
            if (taxDocumentResult.IsFailure)
                return Result<SupplierOutputDto>.Failure(taxDocumentResult.Error);

            var existsResult = await supplierRepository.TaxDocumentExists(
                tenantIdResult.Value,
                taxDocumentResult.Value,
                supplierId);
            if (existsResult.IsFailure)
                return Result<SupplierOutputDto>.Failure(existsResult.Error);

            if (existsResult.Value)
                return Result<SupplierOutputDto>.Failure(
                    $"Tax document '{input.TaxDocument}' is already registered for this tenant.");

            personType = resolvedPersonType;
            taxDocument = taxDocumentResult.Value;
        }

        var contactResult = CreateCustomerUseCase.BuildOptionalContact(input.Email, input.Phone, input.Address);
        if (contactResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(contactResult.Error);

        var (email, phone, address) = contactResult.Value;

        var updateResult = supplier.UpdateInfo(
            name,
            personType,
            taxDocument,
            email,
            phone,
            address,
            updateEmail: input.Email is not null,
            updatePhone: input.Phone is not null,
            updateAddress: input.Address is not null);

        if (updateResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(updateResult.Error);

        if (input.IsActive.HasValue)
        {
            var statusResult = input.IsActive.Value
                ? supplier.Activate()
                : supplier.Deactivate();

            if (statusResult.IsFailure)
                return Result<SupplierOutputDto>.Failure(statusResult.Error);
        }

        var saveResult = await supplierRepository.Save(supplier);
        if (saveResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(saveResult.Error);

        return Result<SupplierOutputDto>.Success(SupplierOutputDto.FromDomain(supplier));
    }
}
