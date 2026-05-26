namespace RetailOps.Core.StoreSettings.Application.DTOs;

public sealed record CreateCashRegisterTerminalInputDto
{
    public required string Name { get; init; }
    public string? Status { get; init; }
    public Guid? AssignedOperatorId { get; init; }
}