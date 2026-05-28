using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Core.Sales.Domain.Services;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class RemoveCartLineUseCase(
    ICashSessionRepository cashSessionRepository,
    ISalesLegacyPort salesLegacyPort,
    CartStockReservationService stockReservation) : IUseCase<(int tenantId, Guid operatorUserId, Guid lineId), CashSessionOutputDto>
{
    public async Task<Result<CashSessionOutputDto>> Execute(
        (int tenantId, Guid operatorUserId, Guid lineId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, operatorUserId, lineId) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(tenantIdResult.Error);

        var sessionResult = await cashSessionRepository.GetOpenByOperator(tenantIdResult.Value, operatorUserId);
        if (sessionResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(sessionResult.Error);

        if (sessionResult.Value is null)
            return Result<CashSessionOutputDto>.Failure("No open cash session found.");

        var session = sessionResult.Value;
        var line = session.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
            return Result<CashSessionOutputDto>.Failure("Cart line not found.");

        if (line.Status == SaleLineStatus.Ready)
        {
            var release = await stockReservation.ReleaseLineAsync(tenantIdResult.Value, line, cancellationToken);
            if (release.IsFailure)
                return Result<CashSessionOutputDto>.Failure(release.Error);
        }

        var removeResult = session.RemoveLine(lineId);
        if (removeResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(removeResult.Error);

        var legacyRemove = await salesLegacyPort.RemoveCartLine(session, lineId, cancellationToken);
        if (legacyRemove.IsFailure)
            return Result<CashSessionOutputDto>.Failure(legacyRemove.Error);

        var saveResult = await cashSessionRepository.Save(session);
        if (saveResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(saveResult.Error);

        return Result<CashSessionOutputDto>.Success(CashSessionOutputDto.FromDomain(session));
    }
}
