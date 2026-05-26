using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class GetCashRegisterTerminalUseCase : IUseCase<(int tenantId, Guid terminalId), CashRegisterTerminalOutputDto>
{
    private readonly ICashRegisterTerminalRepository _terminalRepository;

    public GetCashRegisterTerminalUseCase(ICashRegisterTerminalRepository terminalRepository)
    {
        _terminalRepository = terminalRepository;
    }

    public async Task<Result<CashRegisterTerminalOutputDto>> Execute(
        (int tenantId, Guid terminalId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, terminalId) = request;

        var tenantIdResult = RetailOps.Shared.Kernel.Domain.ValueObjects.TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CashRegisterTerminalOutputDto>.Failure(tenantIdResult.Error);

        var terminalResult = await _terminalRepository.GetById(terminalId);
        if (terminalResult.IsFailure)
            return Result<CashRegisterTerminalOutputDto>.Failure(terminalResult.Error);

        var terminal = terminalResult.Value;

        // Verify tenant ownership
        if (terminal.TenantId.Value != tenantId)
            return Result<CashRegisterTerminalOutputDto>.Failure("Cash register terminal does not belong to this tenant.");

        var outputDto = CashRegisterTerminalOutputDto.FromDomain(terminal);
        return Result<CashRegisterTerminalOutputDto>.Success(outputDto);
    }
}