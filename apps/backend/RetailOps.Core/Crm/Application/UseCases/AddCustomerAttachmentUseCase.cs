using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.UseCases;

public sealed class AddCustomerAttachmentUseCase(ICustomerRepository customerRepository)
    : IUseCase<(int tenantId, Guid customerId, AddCustomerAttachmentInputDto input), CustomerAttachmentOutputDto>
{
    public async Task<Result<CustomerAttachmentOutputDto>> Execute(
        (int tenantId, Guid customerId, AddCustomerAttachmentInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, customerId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CustomerAttachmentOutputDto>.Failure(tenantIdResult.Error);

        var customerResult = await customerRepository.GetById(customerId);
        if (customerResult.IsFailure)
            return Result<CustomerAttachmentOutputDto>.Failure(customerResult.Error);

        var customer = customerResult.Value;
        if (customer.TenantId.Value != tenantId)
            return Result<CustomerAttachmentOutputDto>.Failure("Customer does not belong to this tenant.");

        var nameResult = AttachmentName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<CustomerAttachmentOutputDto>.Failure(nameResult.Error);

        var pathResult = AttachmentPath.Create(input.Path);
        if (pathResult.IsFailure)
            return Result<CustomerAttachmentOutputDto>.Failure(pathResult.Error);

        var attachmentResult = CustomerAttachment.Create(
            tenantIdResult.Value,
            customer.Id,
            nameResult.Value,
            pathResult.Value,
            input.ExpiresAt);

        if (attachmentResult.IsFailure)
            return Result<CustomerAttachmentOutputDto>.Failure(attachmentResult.Error);

        var addResult = customer.AddAttachment(attachmentResult.Value);
        if (addResult.IsFailure)
            return Result<CustomerAttachmentOutputDto>.Failure(addResult.Error);

        var saveResult = await customerRepository.Save(customer);
        if (saveResult.IsFailure)
            return Result<CustomerAttachmentOutputDto>.Failure(saveResult.Error);

        return Result<CustomerAttachmentOutputDto>.Success(
            CustomerAttachmentOutputDto.FromDomain(attachmentResult.Value));
    }
}
