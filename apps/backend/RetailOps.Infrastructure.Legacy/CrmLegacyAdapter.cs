using Microsoft.EntityFrameworkCore;
using RetailOps.Core.Crm.Application.Ports;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Infrastructure.Legacy.Mappers.Crm;
using RetailOps.Infrastructure.Legacy.Persistence;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Infrastructure.Legacy;

public sealed class CrmLegacyAdapter(LegacySasDbContext db) : ICrmLegacyPort
{
    public async Task<Result<IReadOnlyList<Customer>>> GetCustomersFromLegacyAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        try
        {
            var rows = await db.Customers.AsNoTracking()
                .Where(r => r.CompanyId == tenantId.Value)
                .OrderBy(r => r.Nome)
                .ToListAsync(ct);

            var customers = new List<Customer>();
            foreach (var row in rows)
            {
                var customerId = LegacyCrmIds.Customer(row.Id);
                var attachments = await LoadAttachmentsAsync(tenantId, row.Id, customerId, ct);
                var mapped = LegacyCustomerMapper.ToDomain(row, attachments);
                if (mapped.IsSuccess)
                    customers.Add(mapped.Value);
            }

            return Result<IReadOnlyList<Customer>>.Success(customers);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Customer>>.Failure($"Failed to get Customers from legacy: {ex.Message}");
        }
    }

    public async Task<Result<Customer?>> GetCustomerFromLegacyAsync(
        TenantId tenantId,
        Guid customerId,
        CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCrmIds.ParseLegacyId(customerId, "0006");
            if (legacyId is null)
                return Result<Customer?>.Success(null);

            var row = await db.Customers.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result<Customer?>.Success(null);

            var attachments = await LoadAttachmentsAsync(tenantId, row.Id, customerId, ct);
            var result = LegacyCustomerMapper.ToDomain(row, attachments);
            return result.IsFailure
                ? Result<Customer?>.Failure(result.Error)
                : Result<Customer?>.Success(result.Value);
        }
        catch (Exception ex)
        {
            return Result<Customer?>.Failure($"Failed to get Customer from legacy: {ex.Message}");
        }
    }

    public async Task<Result<Customer?>> GetCustomerByCpfFromLegacyAsync(
        TenantId tenantId,
        string cpf,
        CancellationToken ct = default)
    {
        try
        {
            var cleanedCpf = new string(cpf.Where(char.IsDigit).ToArray());
            var row = await db.Customers.AsNoTracking()
                .FirstOrDefaultAsync(r => r.CompanyId == tenantId.Value && r.Cpf == cleanedCpf, ct);

            if (row is null)
                return Result<Customer?>.Success(null);

            var customerId = LegacyCrmIds.Customer(row.Id);
            var attachments = await LoadAttachmentsAsync(tenantId, row.Id, customerId, ct);
            var result = LegacyCustomerMapper.ToDomain(row, attachments);
            return result.IsFailure
                ? Result<Customer?>.Failure(result.Error)
                : Result<Customer?>.Success(result.Value);
        }
        catch (Exception ex)
        {
            return Result<Customer?>.Failure($"Failed to get Customer by CPF from legacy: {ex.Message}");
        }
    }

    public async Task<Result<int>> SaveCustomerToLegacyAsync(Customer customer, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCrmIds.ParseLegacyId(customer.Id, "0006");
            var rowResult = LegacyCustomerMapper.ToLegacy(customer, legacyId);
            if (rowResult.IsFailure)
                return Result<int>.Failure(rowResult.Error);

            var row = rowResult.Value;
            int savedLegacyId;

            if (legacyId is > 0)
            {
                var existing = await db.Customers
                    .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == customer.TenantId.Value, ct);

                if (existing is not null)
                {
                    existing.Nome = row.Nome;
                    existing.Cpf = row.Cpf;
                    existing.Telefone = row.Telefone;
                    existing.Email = row.Email;
                    existing.Endereco = row.Endereco;
                    existing.Ativo = row.Ativo;
                    await db.SaveChangesAsync(ct);
                    savedLegacyId = existing.Id;
                }
                else
                {
                    db.Customers.Add(row);
                    await db.SaveChangesAsync(ct);
                    savedLegacyId = row.Id;
                }
            }
            else
            {
                db.Customers.Add(row);
                await db.SaveChangesAsync(ct);
                savedLegacyId = row.Id;
            }

            customer.SyncIdentity(LegacyCrmIds.Customer(savedLegacyId));

            foreach (var attachment in customer.Attachments)
            {
                var attachmentLegacyId = LegacyCrmIds.ParseLegacyId(attachment.Id, "0008");
                var attachmentRowResult = LegacyAttachmentMapper.ToLegacy(attachment, savedLegacyId, attachmentLegacyId);
                if (attachmentRowResult.IsFailure)
                    return Result<int>.Failure(attachmentRowResult.Error);

                var attachmentRow = attachmentRowResult.Value;

                if (attachmentLegacyId is > 0)
                {
                    var existingAttachment = await db.Attachments
                        .FirstOrDefaultAsync(a => a.Id == attachmentLegacyId && a.CompanyId == customer.TenantId.Value, ct);

                    if (existingAttachment is not null)
                    {
                        existingAttachment.Nome = attachmentRow.Nome;
                        existingAttachment.Foto = attachmentRow.Foto;
                        existingAttachment.DataValidade = attachmentRow.DataValidade;
                        existingAttachment.IdRef = savedLegacyId;
                        await db.SaveChangesAsync(ct);
                        attachment.SyncIdentity(LegacyCrmIds.Attachment(existingAttachment.Id));
                        continue;
                    }
                }

                db.Attachments.Add(attachmentRow);
                await db.SaveChangesAsync(ct);
                attachment.SyncIdentity(LegacyCrmIds.Attachment(attachmentRow.Id));
            }

            return Result<int>.Success(savedLegacyId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save Customer to legacy: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<CustomerAttachment>>> GetCustomerAttachmentsFromLegacyAsync(
        TenantId tenantId,
        Guid customerId,
        CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCrmIds.ParseLegacyId(customerId, "0006");
            if (legacyId is null)
                return Result<IReadOnlyList<CustomerAttachment>>.Success([]);

            var attachments = await LoadAttachmentsAsync(tenantId, legacyId.Value, customerId, ct);
            return Result<IReadOnlyList<CustomerAttachment>>.Success(attachments);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CustomerAttachment>>.Failure(
                $"Failed to get Customer attachments from legacy: {ex.Message}");
        }
    }

    public async Task<Result<int>> SaveCustomerAttachmentToLegacyAsync(
        CustomerAttachment attachment,
        CancellationToken ct = default)
    {
        try
        {
            var customerLegacyId = LegacyCrmIds.ParseLegacyId(attachment.CustomerId, "0006");
            if (customerLegacyId is null)
                return Result<int>.Failure("Invalid customer id for attachment.");

            var legacyId = LegacyCrmIds.ParseLegacyId(attachment.Id, "0008");
            var rowResult = LegacyAttachmentMapper.ToLegacy(attachment, customerLegacyId.Value, legacyId);
            if (rowResult.IsFailure)
                return Result<int>.Failure(rowResult.Error);

            var row = rowResult.Value;

            if (legacyId is > 0)
            {
                var existing = await db.Attachments
                    .FirstOrDefaultAsync(a => a.Id == legacyId && a.CompanyId == attachment.TenantId.Value, ct);

                if (existing is not null)
                {
                    existing.Nome = row.Nome;
                    existing.Foto = row.Foto;
                    existing.DataValidade = row.DataValidade;
                    existing.IdRef = customerLegacyId.Value;
                    await db.SaveChangesAsync(ct);
                    attachment.SyncIdentity(LegacyCrmIds.Attachment(existing.Id));
                    return Result<int>.Success(existing.Id);
                }
            }

            db.Attachments.Add(row);
            await db.SaveChangesAsync(ct);
            attachment.SyncIdentity(LegacyCrmIds.Attachment(row.Id));
            return Result<int>.Success(row.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save Customer attachment to legacy: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<Supplier>>> GetSuppliersFromLegacyAsync(
        TenantId tenantId,
        CancellationToken ct = default)
    {
        try
        {
            var rows = await db.Suppliers.AsNoTracking()
                .Where(r => r.CompanyId == tenantId.Value)
                .OrderBy(r => r.Nome)
                .ToListAsync(ct);

            var suppliers = new List<Supplier>();
            foreach (var row in rows)
            {
                var mapped = LegacySupplierMapper.ToDomain(row);
                if (mapped.IsSuccess)
                    suppliers.Add(mapped.Value);
            }

            return Result<IReadOnlyList<Supplier>>.Success(suppliers);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Supplier>>.Failure($"Failed to get Suppliers from legacy: {ex.Message}");
        }
    }

    public async Task<Result<Supplier?>> GetSupplierFromLegacyAsync(
        TenantId tenantId,
        Guid supplierId,
        CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCrmIds.ParseLegacyId(supplierId, "0007");
            if (legacyId is null)
                return Result<Supplier?>.Success(null);

            var row = await db.Suppliers.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == tenantId.Value, ct);

            if (row is null)
                return Result<Supplier?>.Success(null);

            var result = LegacySupplierMapper.ToDomain(row);
            return result.IsFailure
                ? Result<Supplier?>.Failure(result.Error)
                : Result<Supplier?>.Success(result.Value);
        }
        catch (Exception ex)
        {
            return Result<Supplier?>.Failure($"Failed to get Supplier from legacy: {ex.Message}");
        }
    }

    public async Task<Result<int>> SaveSupplierToLegacyAsync(Supplier supplier, CancellationToken ct = default)
    {
        try
        {
            var legacyId = LegacyCrmIds.ParseLegacyId(supplier.Id, "0007");
            var rowResult = LegacySupplierMapper.ToLegacy(supplier, legacyId);
            if (rowResult.IsFailure)
                return Result<int>.Failure(rowResult.Error);

            var row = rowResult.Value;

            if (legacyId is > 0)
            {
                var existing = await db.Suppliers
                    .FirstOrDefaultAsync(r => r.Id == legacyId && r.CompanyId == supplier.TenantId.Value, ct);

                if (existing is not null)
                {
                    existing.Nome = row.Nome;
                    existing.Pessoa = row.Pessoa;
                    existing.Cpf = row.Cpf;
                    existing.Telefone = row.Telefone;
                    existing.Email = row.Email;
                    existing.Endereco = row.Endereco;
                    existing.Ativo = row.Ativo;
                    await db.SaveChangesAsync(ct);
                    supplier.SyncIdentity(LegacyCrmIds.Supplier(existing.Id));
                    return Result<int>.Success(existing.Id);
                }
            }

            db.Suppliers.Add(row);
            await db.SaveChangesAsync(ct);
            supplier.SyncIdentity(LegacyCrmIds.Supplier(row.Id));
            return Result<int>.Success(row.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save Supplier to legacy: {ex.Message}");
        }
    }

    private async Task<IReadOnlyList<CustomerAttachment>> LoadAttachmentsAsync(
        TenantId tenantId,
        int customerLegacyId,
        Guid customerId,
        CancellationToken ct)
    {
        var rows = await db.Attachments.AsNoTracking()
            .Where(a => a.CompanyId == tenantId.Value
                && a.IdRef == customerLegacyId
                && a.Tipo == LegacyCustomerMapper.AttachmentType)
            .ToListAsync(ct);

        var attachments = new List<CustomerAttachment>();
        foreach (var row in rows)
        {
            var mapped = LegacyAttachmentMapper.ToDomain(row, customerId);
            if (mapped.IsSuccess)
                attachments.Add(mapped.Value);
        }

        return attachments;
    }
}
