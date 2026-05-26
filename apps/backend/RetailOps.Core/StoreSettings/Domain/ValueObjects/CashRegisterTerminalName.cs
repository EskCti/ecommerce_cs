using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.StoreSettings.Domain.ValueObjects;

public record CashRegisterTerminalName
{
    public string Value { get; }

    private CashRegisterTerminalName(string value) => Value = value;

    public static Result<CashRegisterTerminalName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<CashRegisterTerminalName>.Failure("Cash register terminal name is required.");

        var trimmed = value.Trim();
        
        if (trimmed.Length < 2)
            return Result<CashRegisterTerminalName>.Failure("Cash register terminal name must be at least 2 characters long.");
        
        if (trimmed.Length > 30)
            return Result<CashRegisterTerminalName>.Failure("Cash register terminal name cannot exceed 30 characters.");

        return Result<CashRegisterTerminalName>.Success(new CashRegisterTerminalName(trimmed));
    }

    public static implicit operator string(CashRegisterTerminalName terminalName) => terminalName.Value;
}