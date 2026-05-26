using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.UseCases;

public sealed class CreateSupplierUseCase(ISupplierRepository supplierRepository)
    : IUseCase<(int tenantId, CreateSupplierInputDto input), SupplierOutputDto>
{
    public async Task<Result<SupplierOutputDto>> Execute(
        (int tenantId, CreateSupplierInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(tenantIdResult.Error);

        var nameResult = PersonName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(nameResult.Error);

        var taxDocumentResult = TaxDocument.Create(input.TaxDocument, input.PersonType);
        if (taxDocumentResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(taxDocumentResult.Error);

        var existsResult = await supplierRepository.TaxDocumentExists(
            tenantIdResult.Value,
            taxDocumentResult.Value);
        if (existsResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(existsResult.Error);

        if (existsResult.Value)
            return Result<SupplierOutputDto>.Failure(
                $"Tax document '{input.TaxDocument}' is already registered for this tenant.");

        var contactResult = CreateCustomerUseCase.BuildOptionalContact(input.Email, input.Phone, input.Address);
        if (contactResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(contactResult.Error);

        var (email, phone, address) = contactResult.Value;

        var supplierResult = Supplier.Create(
            tenantIdResult.Value,
            nameResult.Value,
            input.PersonType,
            taxDocumentResult.Value,
            email,
            phone,
            address,
            input.IsActive);

        if (supplierResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(supplierResult.Error);

        var saveResult = await supplierRepository.Save(supplierResult.Value);
        if (saveResult.IsFailure)
            return Result<SupplierOutputDto>.Failure(saveResult.Error);

        return Result<SupplierOutputDto>.Success(SupplierOutputDto.FromDomain(supplierResult.Value));
    }
}
