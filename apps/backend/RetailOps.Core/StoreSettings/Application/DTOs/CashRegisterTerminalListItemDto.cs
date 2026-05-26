namespace RetailOps.Core.StoreSettings.Application.DTOs;

public sealed record CashRegisterTerminalListItemDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Status { get; init; }
    public required Guid? AssignedOperatorId { get; init; }
    public required string? AssignedOperatorName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static CashRegisterTerminalListItemDto FromDomain(
        Domain.Entities.CashRegisterTerminal terminal,
        string? assignedOperatorName = null)
    {
        return new CashRegisterTerminalListItemDto
        {
            Id = terminal.Id,
            Name = terminal.Name.Value,
            Status = terminal.Status.ToString(),
            AssignedOperatorId = terminal.AssignedOperatorId?.Value,
            AssignedOperatorName = assignedOperatorName,
            CreatedAt = terminal.CreatedAt,
            UpdatedAt = terminal.UpdatedAt
        };
    }
}