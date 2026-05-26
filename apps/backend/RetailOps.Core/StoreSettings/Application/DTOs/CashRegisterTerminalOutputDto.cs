namespace RetailOps.Core.StoreSettings.Application.DTOs;

public sealed record CashRegisterTerminalOutputDto
{
    public required Guid Id { get; init; }
    public required int TenantId { get; init; }
    public required string Name { get; init; }
    public required string Status { get; init; }
    public Guid? AssignedOperatorId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static CashRegisterTerminalOutputDto FromDomain(Domain.Entities.CashRegisterTerminal terminal)
    {
        return new CashRegisterTerminalOutputDto
        {
            Id = terminal.Id,
            TenantId = terminal.TenantId.Value,
            Name = terminal.Name.Value,
            Status = terminal.Status.ToString(),
            AssignedOperatorId = terminal.AssignedOperatorId?.Value,
            CreatedAt = terminal.CreatedAt,
            UpdatedAt = terminal.UpdatedAt
        };
    }
}