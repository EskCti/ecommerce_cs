namespace RetailOps.Core.Crm.Application.DTOs;

public sealed record CreateCustomerInputDto
{
    public required string Name { get; init; }
    public required string Cpf { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed record UpdateCustomerInputDto
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public bool? IsActive { get; init; }
}

public sealed record CustomerOutputDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required string Name { get; init; }
    public required string Cpf { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required IReadOnlyList<CustomerAttachmentOutputDto> Attachments { get; init; }

    public static CustomerOutputDto FromDomain(Domain.Entities.Customer customer)
    {
        return new CustomerOutputDto
        {
            Id = customer.Id,
            TenantId = customer.TenantId.Value,
            Name = customer.Name.Value,
            Cpf = customer.Cpf.Value,
            Email = customer.Email?.Value,
            Phone = customer.Phone?.Value,
            Address = customer.Address?.Value,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            Attachments = customer.Attachments
                .Select(CustomerAttachmentOutputDto.FromDomain)
                .ToList()
        };
    }
}

public sealed record CustomerListItemDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required string Name { get; init; }
    public required string Cpf { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public required bool IsActive { get; init; }

    public static CustomerListItemDto FromDomain(Domain.Entities.Customer customer)
    {
        return new CustomerListItemDto
        {
            Id = customer.Id,
            TenantId = customer.TenantId.Value,
            Name = customer.Name.Value,
            Cpf = customer.Cpf.Value,
            Email = customer.Email?.Value,
            Phone = customer.Phone?.Value,
            IsActive = customer.IsActive
        };
    }
}

public sealed record FindOrCreateByCpfInputDto
{
    public required string Cpf { get; init; }
    public required string Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

public sealed record FindOrCreateByCpfOutputDto
{
    public required CustomerOutputDto Customer { get; init; }
    public required bool Created { get; init; }
}

public sealed record AddCustomerAttachmentInputDto
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public sealed record CustomerAttachmentOutputDto
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public required DateTime CreatedAt { get; init; }

    public static CustomerAttachmentOutputDto FromDomain(Domain.Entities.CustomerAttachment attachment)
    {
        return new CustomerAttachmentOutputDto
        {
            Id = attachment.Id,
            CustomerId = attachment.CustomerId,
            Name = attachment.Name.Value,
            Path = attachment.Path.Value,
            ExpiresAt = attachment.ExpiresAt,
            CreatedAt = attachment.CreatedAt
        };
    }
}
