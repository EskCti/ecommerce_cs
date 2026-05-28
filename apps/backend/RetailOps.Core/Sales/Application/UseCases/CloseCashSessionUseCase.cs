using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class CloseCashSessionUseCase(
    ICashSessionRepository cashSessionRepository,
    ISalesLegacyPort salesLegacyPort,
    IManagerPinVerifier managerPinVerifier) : IUseCase<(int tenantId, Guid operatorUserId, CloseCashSessionInputDto input), CashSessionOutputDto>
{
    public async Task<Result<CashSessionOutputDto>> Execute(
        (int tenantId, Guid operatorUserId, CloseCashSessionInputDto input) request,
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

        var sessionResult = await cashSessionRepository.GetOpenByOperator(tenantIdResult.Value, operatorUserId);
        if (sessionResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(sessionResult.Error);

        if (sessionResult.Value is null)
            return Result<CashSessionOutputDto>.Failure("No open cash session found.");

        var session = sessionResult.Value;
        var closeResult = session.Close(input.CountedCash, managerPinVerified: true);
        if (closeResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(closeResult.Error);

        var legacyClose = await salesLegacyPort.CloseSession(session, cancellationToken);
        if (legacyClose.IsFailure)
            return Result<CashSessionOutputDto>.Failure(legacyClose.Error);

        var saveResult = await cashSessionRepository.Save(session);
        if (saveResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(saveResult.Error);

        return Result<CashSessionOutputDto>.Success(CashSessionOutputDto.FromDomain(session));
    }
}
