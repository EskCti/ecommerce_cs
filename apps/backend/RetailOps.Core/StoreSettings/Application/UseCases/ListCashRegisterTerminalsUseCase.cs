using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;
using RetailOps.Core.StoreSettings.Application.DTOs;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Entities;

namespace RetailOps.Core.StoreSettings.Application.UseCases;

public sealed class ListCashRegisterTerminalsUseCase : IUseCase<int, IEnumerable<CashRegisterTerminalOutputDto>>
{
    private readonly ICashRegisterTerminalRepository _terminalRepository;

    public ListCashRegisterTerminalsUseCase(ICashRegisterTerminalRepository terminalRepository)
    {
        _terminalRepository = terminalRepository;
    }

    public async Task<Result<IEnumerable<CashRegisterTerminalOutputDto>>> Execute(
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenantIdResult = RetailOps.Shared.Kernel.Domain.ValueObjects.TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<IEnumerable<CashRegisterTerminalOutputDto>>.Failure(tenantIdResult.Error);

        var terminalsResult = await _terminalRepository.GetByTenantId(tenantIdResult.Value);
        if (terminalsResult.IsFailure)
            return Result<IEnumerable<CashRegisterTerminalOutputDto>>.Failure(terminalsResult.Error);

        var terminals = terminalsResult.Value;
        var outputDtos = terminals.Select(CashRegisterTerminalOutputDto.FromDomain);

        return Result<IEnumerable<CashRegisterTerminalOutputDto>>.Success(outputDtos);
    }
}