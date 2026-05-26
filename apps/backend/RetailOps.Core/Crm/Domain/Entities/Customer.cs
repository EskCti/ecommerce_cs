using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.Crm.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Domain.Entities;

public sealed class Customer : Entity
{
    public TenantId TenantId { get; private set; }
    public PersonName Name { get; private set; } = null!;
    public Cpf Cpf { get; private set; } = null!;
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }
    public Address? Address { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<CustomerAttachment> _attachments = [];
    public IReadOnlyList<CustomerAttachment> Attachments => _attachments.AsReadOnly();

    private Customer() { }

    private Customer(
        TenantId tenantId,
        PersonName name,
        Cpf cpf,
        Email? email,
        Phone? phone,
        Address? address,
        bool isActive)
    {
        TenantId = tenantId;
        Name = name;
        Cpf = cpf;
        Email = email;
        Phone = phone;
        Address = address;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Customer> Create(
        TenantId tenantId,
        PersonName name,
        Cpf cpf,
        Email? email = null,
        Phone? phone = null,
        Address? address = null,
        bool isActive = true)
    {
        return Result<Customer>.Success(new Customer(
            tenantId,
            name,
            cpf,
            email,
            phone,
            address,
            isActive));
    }

    public static Result<Customer> Reconstitute(
        Guid id,
        TenantId tenantId,
        PersonName name,
        Cpf cpf,
        Email? email,
        Phone? phone,
        Address? address,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt,
        IEnumerable<CustomerAttachment>? attachments = null)
    {
        var customer = new Customer(tenantId, name, cpf, email, phone, address, isActive)
        {
            Id = id,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        if (attachments is not null)
            customer._attachments.AddRange(attachments);

        return Result<Customer>.Success(customer);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public Result UpdateContactInfo(
        PersonName? name = null,
        Email? email = null,
        Phone? phone = null,
        Address? address = null,
        bool updateEmail = false,
        bool updatePhone = false,
        bool updateAddress = false)
    {
        if (name is not null)
            Name = name;

        if (updateEmail)
            Email = email;

        if (updatePhone)
            Phone = phone;

        if (updateAddress)
            Address = address;

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddAttachment(CustomerAttachment attachment)
    {
        if (attachment.CustomerId != Id && Id != Guid.Empty)
            return Result.Failure("Attachment does not belong to this customer.");

        if (attachment.TenantId.Value != TenantId.Value)
            return Result.Failure("Attachment tenant mismatch.");

        _attachments.Add(attachment);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    internal void SetAttachments(IEnumerable<CustomerAttachment> attachments)
    {
        _attachments.Clear();
        _attachments.AddRange(attachments);
    }
}
