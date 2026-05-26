using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class DeleteCashRegisterTerminalUseCase : IUseCase<(int tenantId, Guid terminalId), bool>
{
    private readonly ICashRegisterTerminalRepository _terminalRepository;

    public DeleteCashRegisterTerminalUseCase(ICashRegisterTerminalRepository terminalRepository)
    {
        _terminalRepository = terminalRepository;
    }

    public async Task<Result<bool>> Execute(
        (int tenantId, Guid terminalId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, terminalId) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<bool>.Failure(tenantIdResult.Error);

        var terminalResult = await _terminalRepository.GetById(terminalId);
        if (terminalResult.IsFailure)
            return Result<bool>.Failure(terminalResult.Error);

        var terminal = terminalResult.Value;
        if (terminal.TenantId.Value != tenantId)
            return Result<bool>.Failure("Cash register terminal does not belong to this tenant.");

        if (terminal.Status == TerminalStatus.Open)
            return Result<bool>.Failure("Cannot delete an open cash register terminal.");

        var deleteResult = await _terminalRepository.Delete(terminalId);
        if (deleteResult.IsFailure)
            return Result<bool>.Failure(deleteResult.Error);

        return Result<bool>.Success(true);
    }
}
