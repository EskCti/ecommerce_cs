using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.Crm.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Domain.Entities;

public sealed class Supplier : Entity
{
    public TenantId TenantId { get; private set; }
    public PersonName Name { get; private set; } = null!;
    public PersonType PersonType { get; private set; }
    public TaxDocument TaxDocument { get; private set; } = null!;
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }
    public Address? Address { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Supplier() { }

    private Supplier(
        TenantId tenantId,
        PersonName name,
        PersonType personType,
        TaxDocument taxDocument,
        Email? email,
        Phone? phone,
        Address? address,
        bool isActive)
    {
        TenantId = tenantId;
        Name = name;
        PersonType = personType;
        TaxDocument = taxDocument;
        Email = email;
        Phone = phone;
        Address = address;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Supplier> Create(
        TenantId tenantId,
        PersonName name,
        PersonType personType,
        TaxDocument taxDocument,
        Email? email = null,
        Phone? phone = null,
        Address? address = null,
        bool isActive = true)
    {
        if (taxDocument.PersonType != personType)
            return Result<Supplier>.Failure("Tax document does not match person type.");

        return Result<Supplier>.Success(new Supplier(
            tenantId,
            name,
            personType,
            taxDocument,
            email,
            phone,
            address,
            isActive));
    }

    public static Result<Supplier> Reconstitute(
        Guid id,
        TenantId tenantId,
        PersonName name,
        PersonType personType,
        TaxDocument taxDocument,
        Email? email,
        Phone? phone,
        Address? address,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var supplier = new Supplier(tenantId, name, personType, taxDocument, email, phone, address, isActive)
        {
            Id = id,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        return Result<Supplier>.Success(supplier);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public Result UpdateInfo(
        PersonName? name = null,
        PersonType? personType = null,
        TaxDocument? taxDocument = null,
        Email? email = null,
        Phone? phone = null,
        Address? address = null,
        bool updateEmail = false,
        bool updatePhone = false,
        bool updateAddress = false)
    {
        if (name is not null)
            Name = name;

        if (personType.HasValue && taxDocument is not null)
        {
            if (taxDocument.PersonType != personType.Value)
                return Result.Failure("Tax document does not match person type.");

            PersonType = personType.Value;
            TaxDocument = taxDocument;
        }
        else if (personType.HasValue || taxDocument is not null)
        {
            return Result.Failure("Person type and tax document must be updated together.");
        }

        if (updateEmail)
            Email = email;

        if (updatePhone)
            Phone = phone;

        if (updateAddress)
            Address = address;

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
}
