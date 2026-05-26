using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class UpdateCashRegisterTerminalUseCase : IUseCase<(int tenantId, Guid terminalId, UpdateCashRegisterTerminalInputDto input), CashRegisterTerminalOutputDto>
{
    private readonly ICashRegisterTerminalRepository _terminalRepository;

    public UpdateCashRegisterTerminalUseCase(ICashRegisterTerminalRepository terminalRepository)
    {
        _terminalRepository = terminalRepository;
    }

    public async Task<Result<CashRegisterTerminalOutputDto>> Execute(
        (int tenantId, Guid terminalId, UpdateCashRegisterTerminalInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, terminalId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CashRegisterTerminalOutputDto>.Failure(tenantIdResult.Error);

        var terminalResult = await _terminalRepository.GetById(terminalId);
        if (terminalResult.IsFailure)
            return Result<CashRegisterTerminalOutputDto>.Failure(terminalResult.Error);

        var terminal = terminalResult.Value;

        // Verify tenant ownership
        if (terminal.TenantId.Value != tenantId)
            return Result<CashRegisterTerminalOutputDto>.Failure("Cash register terminal does not belong to this tenant.");

        // Update name if provided
        if (input.Name != null)
        {
            var nameResult = CashRegisterTerminalName.Create(input.Name);
            if (nameResult.IsFailure)
                return Result<CashRegisterTerminalOutputDto>.Failure(nameResult.Error);

            // Check if new name already exists (excluding current terminal)
            var existingTerminalResult = await _terminalRepository.GetByName(tenantIdResult.Value, nameResult.Value);
            if (existingTerminalResult.IsSuccess && existingTerminalResult.Value.Id != terminalId)
                return Result<CashRegisterTerminalOutputDto>.Failure($"Cash register terminal name '{input.Name}' already exists for this tenant.");

            var renameResult = terminal.Rename(nameResult.Value);
            if (renameResult.IsFailure)
                return Result<CashRegisterTerminalOutputDto>.Failure(renameResult.Error);
        }

        // Update status if provided
        if (input.Status != null)
        {
            if (!Enum.TryParse<TerminalStatus>(input.Status, true, out var parsedStatus))
                return Result<CashRegisterTerminalOutputDto>.Failure($"Invalid terminal status: {input.Status}");

            var statusUpdateResult = UpdateTerminalStatus(terminal, parsedStatus);
            if (statusUpdateResult.IsFailure)
                return Result<CashRegisterTerminalOutputDto>.Failure(statusUpdateResult.Error);
        }

        // Update assigned operator if provided
        if (input.AssignedOperatorId.HasValue)
        {
            var userIdResult = UserId.Create(input.AssignedOperatorId.Value);
            if (userIdResult.IsFailure)
                return Result<CashRegisterTerminalOutputDto>.Failure(userIdResult.Error);

            var assignResult = terminal.AssignOperator(userIdResult.Value);
            if (assignResult.IsFailure)
                return Result<CashRegisterTerminalOutputDto>.Failure(assignResult.Error);
        }
        else if (input.AssignedOperatorId == null && input.AssignedOperatorId != terminal.AssignedOperatorId?.Value)
        {
            // Explicit null means unassign operator
            var unassignResult = terminal.UnassignOperator();
            if (unassignResult.IsFailure)
                return Result<CashRegisterTerminalOutputDto>.Failure(unassignResult.Error);
        }

        var saveResult = await _terminalRepository.Save(terminal);
        if (saveResult.IsFailure)
            return Result<CashRegisterTerminalOutputDto>.Failure(saveResult.Error);

        var outputDto = CashRegisterTerminalOutputDto.FromDomain(terminal);
        return Result<CashRegisterTerminalOutputDto>.Success(outputDto);
    }

    private Result UpdateTerminalStatus(CashRegisterTerminal terminal, TerminalStatus newStatus)
    {
        return newStatus switch
        {
            TerminalStatus.Open => terminal.Open(terminal.AssignedOperatorId),
            TerminalStatus.Closed => terminal.Close(),
            TerminalStatus.Maintenance => terminal.PutInMaintenance(),
            TerminalStatus.OutOfService => terminal.TakeOutOfService(),
            _ => Result.Failure($"Unsupported terminal status: {newStatus}")
        };
    }
}