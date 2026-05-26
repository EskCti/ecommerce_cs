using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class CreateCashRegisterTerminalUseCase : IUseCase<(int tenantId, CreateCashRegisterTerminalInputDto input), CashRegisterTerminalOutputDto>
{
    private readonly ICashRegisterTerminalRepository _terminalRepository;

    public CreateCashRegisterTerminalUseCase(ICashRegisterTerminalRepository terminalRepository)
    {
        _terminalRepository = terminalRepository;
    }

    public async Task<Result<CashRegisterTerminalOutputDto>> Execute(
        (int tenantId, CreateCashRegisterTerminalInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CashRegisterTerminalOutputDto>.Failure(tenantIdResult.Error);

        var nameResult = CashRegisterTerminalName.Create(input.Name);
        if (nameResult.IsFailure)
            return Result<CashRegisterTerminalOutputDto>.Failure(nameResult.Error);

        // Check if name already exists for this tenant
        var existingTerminalResult = await _terminalRepository.GetByName(tenantIdResult.Value, nameResult.Value);
        if (existingTerminalResult.IsSuccess)
            return Result<CashRegisterTerminalOutputDto>.Failure($"Cash register terminal name '{input.Name}' already exists for this tenant.");

        // Parse status if provided, otherwise default to Closed
        TerminalStatus status = TerminalStatus.Closed;
        if (input.Status != null)
        {
            if (!Enum.TryParse<TerminalStatus>(input.Status, true, out var parsedStatus))
                return Result<CashRegisterTerminalOutputDto>.Failure($"Invalid terminal status: {input.Status}");

            status = parsedStatus;
        }

        // Create UserId if assigned operator provided
        UserId? assignedOperatorId = null;
        if (input.AssignedOperatorId.HasValue)
        {
            var userIdResult = UserId.Create(input.AssignedOperatorId.Value);
            if (userIdResult.IsFailure)
                return Result<CashRegisterTerminalOutputDto>.Failure(userIdResult.Error);

            assignedOperatorId = userIdResult.Value;
        }

        var terminalResult = CashRegisterTerminal.Create(
            tenantIdResult.Value,
            nameResult.Value,
            status,
            assignedOperatorId);

        if (terminalResult.IsFailure)
            return Result<CashRegisterTerminalOutputDto>.Failure(terminalResult.Error);

        var terminal = terminalResult.Value;
        var saveResult = await _terminalRepository.Save(terminal);
        
        if (saveResult.IsFailure)
            return Result<CashRegisterTerminalOutputDto>.Failure(saveResult.Error);

        var outputDto = CashRegisterTerminalOutputDto.FromDomain(terminal);
        return Result<CashRegisterTerminalOutputDto>.Success(outputDto);
    }
}