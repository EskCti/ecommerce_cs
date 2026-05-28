using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Core.Sales.Domain.Services;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class ConfirmGradeForItemUseCase(
    ICashSessionRepository cashSessionRepository,
    ISalesLegacyPort salesLegacyPort,
    CartStockReservationService stockReservation) : IUseCase<(int tenantId, Guid operatorUserId, Guid lineId, ConfirmGradeForItemInputDto input), CashSessionOutputDto>
{
    public async Task<Result<CashSessionOutputDto>> Execute(
        (int tenantId, Guid operatorUserId, Guid lineId, ConfirmGradeForItemInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, operatorUserId, lineId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(tenantIdResult.Error);

        var sessionResult = await cashSessionRepository.GetOpenByOperator(tenantIdResult.Value, operatorUserId);
        if (sessionResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(sessionResult.Error);

        if (sessionResult.Value is null)
            return Result<CashSessionOutputDto>.Failure("No open cash session found.");

        var session = sessionResult.Value;
        var confirmResult = session.ConfirmGradeForLine(lineId, input.GradeOptionIds);
        if (confirmResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(confirmResult.Error);

        var line = session.Lines.First(l => l.Id == lineId);
        var reserve = await stockReservation.ReserveLineAsync(tenantIdResult.Value, line, cancellationToken);
        if (reserve.IsFailure)
            return Result<CashSessionOutputDto>.Failure(reserve.Error);

        var saveLegacy = await salesLegacyPort.SaveSession(session, cancellationToken);
        if (saveLegacy.IsFailure)
            return Result<CashSessionOutputDto>.Failure(saveLegacy.Error);

        var saveResult = await cashSessionRepository.Save(session);
        if (saveResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(saveResult.Error);

        return Result<CashSessionOutputDto>.Success(CashSessionOutputDto.FromDomain(session));
    }
}
