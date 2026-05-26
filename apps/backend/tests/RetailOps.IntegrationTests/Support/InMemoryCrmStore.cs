using RetailOps.Core.Crm.Application.DTOs;
using RetailOps.Core.Crm.Application.Ports;
using RetailOps.Core.Crm.Application.Queries;
using RetailOps.Core.Crm.Domain.Entities;
using RetailOps.Core.Crm.Domain.Repositories;
using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.IntegrationTests.Support;

internal sealed class CrmTestState
{
    public Dictionary<Guid, Customer> Customers { get; } = new();
    public Dictionary<Guid, Supplier> Suppliers { get; } = new();
    public Dictionary<Guid, CustomerAttachment> Attachments { get; } = new();
    public int CustomerLegacyId { get; set; }
    public int SupplierLegacyId { get; set; }
    public int AttachmentLegacyId { get; set; }

    public void Reset()
    {
        Customers.Clear();
        Suppliers.Clear();
        Attachments.Clear();
        CustomerLegacyId = 0;
        SupplierLegacyId = 0;
        AttachmentLegacyId = 0;
    }
}

internal sealed class InMemoryCustomerRepository(CrmTestState state) : ICustomerRepository
{
    public Task<Result<Customer>> GetById(Guid id) =>
        Task.FromResult(state.Customers.TryGetValue(id, out var customer)
            ? Result<Customer>.Success(customer)
            : Result<Customer>.Failure("Customer not found."));

    public Task<Result<Customer?>> GetByCpf(TenantId tenantId, Cpf cpf)
    {
        var match = state.Customers.Values.FirstOrDefault(c =>
            c.TenantId.Value == tenantId.Value && c.Cpf.Value == cpf.Value);

        return Task.FromResult(Result<Customer?>.Success(match));
    }

    public Task<Result<bool>> CpfExists(TenantId tenantId, Cpf cpf, Guid? excludeCustomerId = null)
    {
        var exists = state.Customers.Values.Any(c =>
            c.TenantId.Value == tenantId.Value
            && c.Cpf.Value == cpf.Value
            && (excludeCustomerId is null || c.Id != excludeCustomerId));

        return Task.FromResult(Result<bool>.Success(exists));
    }

    public Task<Result<IEnumerable<Customer>>> GetByTenantId(TenantId tenantId)
    {
        var items = state.Customers.Values.Where(c => c.TenantId.Value == tenantId.Value);
        return Task.FromResult(Result<IEnumerable<Customer>>.Success(items));
    }

    public Task<Result> Save(Customer entity)
    {
        if (!state.Customers.ContainsKey(entity.Id))
        {
            state.CustomerLegacyId++;
            entity.SyncIdentity(Guid.Parse($"00000000-0000-0000-0006-{state.CustomerLegacyId:D12}"));
        }

        foreach (var attachment in entity.Attachments)
        {
            if (!state.Attachments.ContainsKey(attachment.Id))
            {
                state.AttachmentLegacyId++;
                attachment.SyncIdentity(Guid.Parse($"00000000-0000-0000-0008-{state.AttachmentLegacyId:D12}"));
            }

            state.Attachments[attachment.Id] = attachment;
        }

        state.Customers[entity.Id] = entity;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Customer cannot be deleted. Use deactivation instead."));
}

internal sealed class InMemorySupplierRepository(CrmTestState state) : ISupplierRepository
{
    public Task<Result<Supplier>> GetById(Guid id) =>
        Task.FromResult(state.Suppliers.TryGetValue(id, out var supplier)
            ? Result<Supplier>.Success(supplier)
            : Result<Supplier>.Failure("Supplier not found."));

    public Task<Result<IEnumerable<Supplier>>> GetByTenantId(TenantId tenantId)
    {
        var items = state.Suppliers.Values.Where(s => s.TenantId.Value == tenantId.Value);
        return Task.FromResult(Result<IEnumerable<Supplier>>.Success(items));
    }

    public Task<Result<bool>> TaxDocumentExists(
        TenantId tenantId,
        TaxDocument taxDocument,
        Guid? excludeSupplierId = null)
    {
        var exists = state.Suppliers.Values.Any(s =>
            s.TenantId.Value == tenantId.Value
            && s.TaxDocument.Value == taxDocument.Value
            && (excludeSupplierId is null || s.Id != excludeSupplierId));

        return Task.FromResult(Result<bool>.Success(exists));
    }

    public Task<Result> Save(Supplier entity)
    {
        if (!state.Suppliers.ContainsKey(entity.Id))
        {
            state.SupplierLegacyId++;
            entity.SyncIdentity(Guid.Parse($"00000000-0000-0000-0007-{state.SupplierLegacyId:D12}"));
        }

        state.Suppliers[entity.Id] = entity;
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Delete(Guid id) =>
        Task.FromResult(Result.Failure("Supplier cannot be deleted. Use deactivation instead."));
}

internal sealed class InMemoryCrmLegacyPort(CrmTestState state) : ICrmLegacyPort
{
    public Task<Result<IReadOnlyList<Customer>>> GetCustomersFromLegacyAsync(TenantId tenantId, CancellationToken ct = default)
    {
        var items = state.Customers.Values.Where(c => c.TenantId.Value == tenantId.Value).ToList();
        return Task.FromResult(Result<IReadOnlyList<Customer>>.Success(items));
    }

    public Task<Result<Customer?>> GetCustomerFromLegacyAsync(TenantId tenantId, Guid customerId, CancellationToken ct = default)
    {
        if (!state.Customers.TryGetValue(customerId, out var customer) || customer.TenantId.Value != tenantId.Value)
            return Task.FromResult(Result<Customer?>.Success(null));

        return Task.FromResult(Result<Customer?>.Success(customer));
    }

    public Task<Result<Customer?>> GetCustomerByCpfFromLegacyAsync(TenantId tenantId, string cpf, CancellationToken ct = default)
    {
        var cleaned = new string(cpf.Where(char.IsDigit).ToArray());
        var match = state.Customers.Values.FirstOrDefault(c =>
            c.TenantId.Value == tenantId.Value && c.Cpf.Value == cleaned);

        return Task.FromResult(Result<Customer?>.Success(match));
    }

    public Task<Result<int>> SaveCustomerToLegacyAsync(Customer customer, CancellationToken ct = default) =>
        Task.FromResult(Result<int>.Success(1));

    public Task<Result<IReadOnlyList<CustomerAttachment>>> GetCustomerAttachmentsFromLegacyAsync(
        TenantId tenantId,
        Guid customerId,
        CancellationToken ct = default)
    {
        var items = state.Attachments.Values
            .Where(a => a.TenantId.Value == tenantId.Value && a.CustomerId == customerId)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CustomerAttachment>>.Success(items));
    }

    public Task<Result<int>> SaveCustomerAttachmentToLegacyAsync(CustomerAttachment attachment, CancellationToken ct = default) =>
        Task.FromResult(Result<int>.Success(1));

    public Task<Result<IReadOnlyList<Supplier>>> GetSuppliersFromLegacyAsync(TenantId tenantId, CancellationToken ct = default)
    {
        var items = state.Suppliers.Values.Where(s => s.TenantId.Value == tenantId.Value).ToList();
        return Task.FromResult(Result<IReadOnlyList<Supplier>>.Success(items));
    }

    public Task<Result<Supplier?>> GetSupplierFromLegacyAsync(TenantId tenantId, Guid supplierId, CancellationToken ct = default)
    {
        if (!state.Suppliers.TryGetValue(supplierId, out var supplier) || supplier.TenantId.Value != tenantId.Value)
            return Task.FromResult(Result<Supplier?>.Success(null));

        return Task.FromResult(Result<Supplier?>.Success(supplier));
    }

    public Task<Result<int>> SaveSupplierToLegacyAsync(Supplier supplier, CancellationToken ct = default) =>
        Task.FromResult(Result<int>.Success(1));
}

internal sealed class InMemoryCustomerQueries(CrmTestState state, ICrmLegacyPort legacyPort) : ICustomerQueries
{
    public async Task<Result<IReadOnlyList<CustomerListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        CustomerListFilter? filter = null,
        CancellationToken ct = default)
    {
        var items = state.Customers.Values
            .Where(c => c.TenantId.Value == tenantId.Value)
            .Select(CustomerListItemDto.FromDomain)
            .ToList();

        return await Task.FromResult(Result<IReadOnlyList<CustomerListItemDto>>.Success(items));
    }

    public async Task<Result<IReadOnlyList<CustomerAttachmentOutputDto>>> ListAttachmentsAsync(
        TenantId tenantId,
        Guid customerId,
        CancellationToken ct = default)
    {
        var result = await legacyPort.GetCustomerAttachmentsFromLegacyAsync(tenantId, customerId, ct);
        if (result.IsFailure)
            return Result<IReadOnlyList<CustomerAttachmentOutputDto>>.Failure(result.Error);

        var projected = result.Value.Select(CustomerAttachmentOutputDto.FromDomain).ToList();
        return Result<IReadOnlyList<CustomerAttachmentOutputDto>>.Success(projected);
    }
}

internal sealed class InMemorySupplierQueries(CrmTestState state) : ISupplierQueries
{
    public Task<Result<IReadOnlyList<SupplierListItemDto>>> ListByTenantAsync(
        TenantId tenantId,
        SupplierListFilter? filter = null,
        CancellationToken ct = default)
    {
        var items = state.Suppliers.Values
            .Where(s => s.TenantId.Value == tenantId.Value)
            .Select(SupplierListItemDto.FromDomain)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<SupplierListItemDto>>.Success(items));
    }
}

internal sealed class InMemoryCrmFixture
{
    public CrmTestState State { get; } = new();
    public InMemoryCustomerRepository CustomerRepository { get; }
    public InMemorySupplierRepository SupplierRepository { get; }
    public InMemoryCrmLegacyPort LegacyPort { get; }
    public InMemoryCustomerQueries CustomerQueries { get; }
    public InMemorySupplierQueries SupplierQueries { get; }

    public InMemoryCrmFixture()
    {
        LegacyPort = new InMemoryCrmLegacyPort(State);
        CustomerRepository = new InMemoryCustomerRepository(State);
        SupplierRepository = new InMemorySupplierRepository(State);
        CustomerQueries = new InMemoryCustomerQueries(State, LegacyPort);
        SupplierQueries = new InMemorySupplierQueries(State);
    }

    public void Reset() => State.Reset();
}
