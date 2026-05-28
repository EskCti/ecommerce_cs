using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Events;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.Repositories;
using RetailOps.Core.StoreSettings.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class OpenCashSessionUseCase(
    ICashSessionRepository cashSessionRepository,
    ICashRegisterTerminalRepository terminalRepository,
    ISalesLegacyPort salesLegacyPort,
    IManagerPinVerifier managerPinVerifier) : IUseCase<(int tenantId, Guid operatorUserId, OpenCashSessionInputDto input), CashSessionOutputDto>
{
    public async Task<Result<CashSessionOutputDto>> Execute(
        (int tenantId, Guid operatorUserId, OpenCashSessionInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, operatorUserId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(tenantIdResult.Error);

        var pinResult = await managerPinVerifier.VerifyAsync(
            tenantIdResult.Value,
            input.ManagerUserId,
            input.ManagerPin,
            cancellationToken);

        if (pinResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(pinResult.Error);

        var terminalResult = await terminalRepository.GetById(input.TerminalId);
        if (terminalResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(terminalResult.Error);

        if (terminalResult.Value.TenantId.Value != tenantId)
            return Result<CashSessionOutputDto>.Failure("Terminal does not belong to this tenant.");

        if (terminalResult.Value.Status != TerminalStatus.Open)
            return Result<CashSessionOutputDto>.Failure("Terminal must be open before starting a cash session.");

        var existingOperator = await cashSessionRepository.GetOpenByOperator(tenantIdResult.Value, operatorUserId);
        if (existingOperator.IsFailure)
            return Result<CashSessionOutputDto>.Failure(existingOperator.Error);

        if (existingOperator.Value is not null)
            return Result<CashSessionOutputDto>.Failure("Operator already has an open cash session.");

        var existingTerminal = await cashSessionRepository.GetOpenByTerminal(tenantIdResult.Value, input.TerminalId);
        if (existingTerminal.IsFailure)
            return Result<CashSessionOutputDto>.Failure(existingTerminal.Error);

        if (existingTerminal.Value is not null)
            return Result<CashSessionOutputDto>.Failure("Terminal already has an open cash session.");

        var sessionResult = CashSession.Open(
            tenantIdResult.Value,
            input.TerminalId,
            operatorUserId,
            input.OpeningFloat,
            managerPinVerified: true);

        if (sessionResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(sessionResult.Error);

        var session = sessionResult.Value;
        var legacyOpen = await salesLegacyPort.OpenSession(session, cancellationToken);
        if (legacyOpen.IsFailure)
            return Result<CashSessionOutputDto>.Failure(legacyOpen.Error);

        var saveResult = await cashSessionRepository.Save(session);
        if (saveResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(saveResult.Error);

        _ = new CashSessionOpenedEvent(
            session.Id,
            tenantId,
            session.TerminalId,
            session.OperatorUserId,
            session.OpeningFloat,
            session.OpenedAt);

        return Result<CashSessionOutputDto>.Success(CashSessionOutputDto.FromDomain(session));
    }
}
