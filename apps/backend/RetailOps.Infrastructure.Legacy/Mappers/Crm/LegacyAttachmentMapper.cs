using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Infrastructure.Legacy.Persistence.Entities;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy.Mappers.Crm;

public static class LegacyAttachmentMapper
{
    public static Result<CustomerAttachment> ToDomain(LegacyAttachmentRow row, Guid customerId)
    {
        try
        {
            var tenantIdResult = TenantId.Create(row.CompanyId);
            if (tenantIdResult.IsFailure)
                return Result<CustomerAttachment>.Failure(tenantIdResult.Error);

            if (string.IsNullOrWhiteSpace(row.Nome))
                return Result<CustomerAttachment>.Failure("Attachment name is required.");

            if (string.IsNullOrWhiteSpace(row.Foto))
                return Result<CustomerAttachment>.Failure("Attachment path is required.");

            var nameResult = AttachmentName.Create(row.Nome);
            if (nameResult.IsFailure)
                return Result<CustomerAttachment>.Failure(nameResult.Error);

            var pathResult = AttachmentPath.Create(row.Foto);
            if (pathResult.IsFailure)
                return Result<CustomerAttachment>.Failure(pathResult.Error);

            return CustomerAttachment.Reconstitute(
                LegacyCrmIds.Attachment(row.Id),
                tenantIdResult.Value,
                customerId,
                nameResult.Value,
                pathResult.Value,
                row.DataValidade,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            return Result<CustomerAttachment>.Failure($"Failed to map legacy Attachment: {ex.Message}");
        }
    }

    public static Result<LegacyAttachmentRow> ToLegacy(
        CustomerAttachment attachment,
        int customerLegacyId,
        int? legacyId = null)
    {
        try
        {
            var row = new LegacyAttachmentRow
            {
                Id = legacyId ?? LegacyCrmIds.ParseLegacyId(attachment.Id, "0008") ?? 0,
                CompanyId = attachment.TenantId.Value,
                Tipo = LegacyCustomerMapper.AttachmentType,
                IdRef = customerLegacyId,
                Nome = attachment.Name.Value,
                Foto = attachment.Path.Value,
                DataValidade = attachment.ExpiresAt
            };

            return Result<LegacyAttachmentRow>.Success(row);
        }
        catch (Exception ex)
        {
            return Result<LegacyAttachmentRow>.Failure($"Failed to map Attachment to legacy: {ex.Message}");
        }
    }
}
