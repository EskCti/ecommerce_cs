using RetailOps.Core.Sales.Domain.Services;
using RetailOps.Core.Sales.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Domain.Entities;

public sealed class CashSession : Entity
{
    public TenantId TenantId { get; private set; }
    public Guid TerminalId { get; private set; }
    public Guid OperatorUserId { get; private set; }
    public CashSessionStatus Status { get; private set; }
    public decimal OpeningFloat { get; private set; }
    public decimal TotalSold { get; private set; }
    public decimal? CountedCash { get; private set; }
    public CashBreakage? Breakage { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    private readonly List<SaleLine> _lines = [];
    private readonly List<CashWithdrawal> _withdrawals = [];

    public IReadOnlyList<SaleLine> Lines => _lines.AsReadOnly();
    public IReadOnlyList<CashWithdrawal> Withdrawals => _withdrawals.AsReadOnly();
    public decimal TotalWithdrawals => _withdrawals.Sum(w => w.Amount);

    private CashSession() { }

    private CashSession(
        TenantId tenantId,
        Guid terminalId,
        Guid operatorUserId,
        decimal openingFloat)
    {
        TenantId = tenantId;
        TerminalId = terminalId;
        OperatorUserId = operatorUserId;
        OpeningFloat = openingFloat;
        Status = CashSessionStatus.Open;
        OpenedAt = DateTime.UtcNow;
    }

    public static Result<CashSession> Open(
        TenantId tenantId,
        Guid terminalId,
        Guid operatorUserId,
        decimal openingFloat,
        bool managerPinVerified)
    {
        if (!managerPinVerified)
            return Result<CashSession>.Failure("Manager PIN verification is required.");

        if (terminalId == Guid.Empty)
            return Result<CashSession>.Failure("Terminal id is required.");

        if (operatorUserId == Guid.Empty)
            return Result<CashSession>.Failure("Operator id is required.");

        if (openingFloat < 0)
            return Result<CashSession>.Failure("Opening float cannot be negative.");

        return Result<CashSession>.Success(new CashSession(tenantId, terminalId, operatorUserId, openingFloat));
    }

    public static Result<CashSession> Reconstitute(
        Guid id,
        TenantId tenantId,
        Guid terminalId,
        Guid operatorUserId,
        CashSessionStatus status,
        decimal openingFloat,
        decimal totalSold,
        decimal? countedCash,
        CashBreakage? breakage,
        DateTime openedAt,
        DateTime? closedAt,
        IEnumerable<SaleLine>? lines,
        IEnumerable<CashWithdrawal>? withdrawals)
    {
        var session = new CashSession(tenantId, terminalId, operatorUserId, openingFloat)
        {
            Id = id,
            Status = status,
            TotalSold = totalSold,
            CountedCash = countedCash,
            Breakage = breakage,
            OpenedAt = openedAt,
            ClosedAt = closedAt
        };

        if (lines is not null)
            session._lines.AddRange(lines);

        if (withdrawals is not null)
            session._withdrawals.AddRange(withdrawals);

        return Result<CashSession>.Success(session);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public Result AddLine(SaleLine line)
    {
        if (Status != CashSessionStatus.Open)
            return Result.Failure("Cannot add lines to a closed cash session.");

        _lines.Add(line);
        return Result.Success();
    }

    public Result RemoveLine(Guid lineId)
    {
        if (Status != CashSessionStatus.Open)
            return Result.Failure("Cannot remove lines from a closed cash session.");

        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
            return Result.Failure("Cart line not found.");

        _lines.Remove(line);
        return Result.Success();
    }

    public Result ConfirmGradeForLine(Guid lineId, IEnumerable<Guid> gradeOptionIds)
    {
        if (Status != CashSessionStatus.Open)
            return Result.Failure("Cannot confirm grade on a closed cash session.");

        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
            return Result.Failure("Cart line not found.");

        return line.ConfirmGrade(gradeOptionIds);
    }

    public Result RegisterWithdrawal(CashWithdrawal withdrawal)
    {
        if (Status != CashSessionStatus.Open)
            return Result.Failure("Cannot register withdrawal on a closed cash session.");

        _withdrawals.Add(withdrawal);
        return Result.Success();
    }

    public Result Close(decimal countedCash, bool managerPinVerified)
    {
        if (!managerPinVerified)
            return Result.Failure("Manager PIN verification is required.");

        if (Status != CashSessionStatus.Open)
            return Result.Failure("Cash session is not open.");

        if (countedCash < 0)
            return Result.Failure("Counted cash cannot be negative.");

        var breakageResult = CashSessionClosingPolicy.CalculateBreakage(
            countedCash,
            OpeningFloat,
            TotalSold,
            TotalWithdrawals);

        if (breakageResult.IsFailure)
            return Result.Failure(breakageResult.Error);

        CountedCash = countedCash;
        Breakage = breakageResult.Value;
        Status = CashSessionStatus.Closed;
        ClosedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void RecordSaleTotal(decimal amount) => TotalSold += Math.Round(amount, 2);

    public bool HasReadyLines() => _lines.Any(l => l.Status == SaleLineStatus.Ready);

    public bool IsCartEmpty() => _lines.Count == 0;
}
