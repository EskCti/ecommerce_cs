using RetailOps.Core.Crm.Domain.ValueObjects;

namespace RetailOps.Core.Crm.Application.DTOs;

public sealed record CreateSupplierInputDto
{
    public required string Name { get; init; }
    public required PersonType PersonType { get; init; }
    public required string TaxDocument { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed record UpdateSupplierInputDto
{
    public string? Name { get; init; }
    public PersonType? PersonType { get; init; }
    public string? TaxDocument { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public bool? IsActive { get; init; }
}

public sealed record SupplierOutputDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required string Name { get; init; }
    public required PersonType PersonType { get; init; }
    public required string TaxDocument { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static SupplierOutputDto FromDomain(Domain.Entities.Supplier supplier)
    {
        return new SupplierOutputDto
        {
            Id = supplier.Id,
            TenantId = supplier.TenantId.Value,
            Name = supplier.Name.Value,
            PersonType = supplier.PersonType,
            TaxDocument = supplier.TaxDocument.Value,
            Email = supplier.Email?.Value,
            Phone = supplier.Phone?.Value,
            Address = supplier.Address?.Value,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt
        };
    }
}

public sealed record SupplierListItemDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required string Name { get; init; }
    public required PersonType PersonType { get; init; }
    public required string TaxDocument { get; init; }
    public string? Email { get; init; }
    public required bool IsActive { get; init; }

    public static SupplierListItemDto FromDomain(Domain.Entities.Supplier supplier)
    {
        return new SupplierListItemDto
        {
            Id = supplier.Id,
            TenantId = supplier.TenantId.Value,
            Name = supplier.Name.Value,
            PersonType = supplier.PersonType,
            TaxDocument = supplier.TaxDocument.Value,
            Email = supplier.Email?.Value,
            IsActive = supplier.IsActive
        };
    }
}
