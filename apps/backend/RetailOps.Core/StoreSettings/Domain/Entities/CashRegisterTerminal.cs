using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Domain.Entities;

public sealed class CashRegisterTerminal : Entity
{
    public TenantId TenantId { get; private set; }
    public CashRegisterTerminalName Name { get; private set; } = null!;
    public TerminalStatus Status { get; private set; }
    public UserId? AssignedOperatorId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CashRegisterTerminal() { }

    private CashRegisterTerminal(
        TenantId tenantId,
        CashRegisterTerminalName name,
        TerminalStatus status,
        UserId? assignedOperatorId)
    {
        TenantId = tenantId;
        Name = name;
        Status = status;
        AssignedOperatorId = assignedOperatorId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<CashRegisterTerminal> Create(
        TenantId tenantId,
        CashRegisterTerminalName name,
        TerminalStatus status = TerminalStatus.Closed,
        UserId? assignedOperatorId = null)
    {
        return Result<CashRegisterTerminal>.Success(new CashRegisterTerminal(
            tenantId,
            name,
            status,
            assignedOperatorId));
    }

    public static Result<CashRegisterTerminal> Reconstitute(
        Guid id,
        TenantId tenantId,
        CashRegisterTerminalName name,
        TerminalStatus status,
        UserId? assignedOperatorId,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var terminal = new CashRegisterTerminal(tenantId, name, status, assignedOperatorId)
        {
            Id = id,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        return Result<CashRegisterTerminal>.Success(terminal);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public Result Rename(CashRegisterTerminalName newName)
    {
        Name = newName;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Open(UserId? operatorId = null)
    {
        if (Status == TerminalStatus.Open)
            return Result.Failure("Terminal is already open.");

        if (Status == TerminalStatus.Maintenance)
            return Result.Failure("Cannot open terminal while in maintenance.");

        if (Status == TerminalStatus.OutOfService)
            return Result.Failure("Cannot open terminal that is out of service.");

        Status = TerminalStatus.Open;
        AssignedOperatorId = operatorId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Close()
    {
        if (Status == TerminalStatus.Closed)
            return Result.Failure("Terminal is already closed.");

        Status = TerminalStatus.Closed;
        AssignedOperatorId = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result PutInMaintenance()
    {
        if (Status == TerminalStatus.Open)
            return Result.Failure("Cannot put open terminal in maintenance. Close it first.");

        Status = TerminalStatus.Maintenance;
        AssignedOperatorId = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result TakeOutOfService()
    {
        if (Status == TerminalStatus.Open)
            return Result.Failure("Cannot take open terminal out of service. Close it first.");

        Status = TerminalStatus.OutOfService;
        AssignedOperatorId = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AssignOperator(UserId operatorId)
    {
        if (Status != TerminalStatus.Open)
            return Result.Failure("Can only assign operator to open terminal.");

        AssignedOperatorId = operatorId;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UnassignOperator()
    {
        if (Status != TerminalStatus.Open)
            return Result.Failure("Can only unassign operator from open terminal.");

        AssignedOperatorId = null;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}