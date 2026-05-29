using RetailOps.Core.Catalog.Application.UseCases;
using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Core.Sales.Domain.Services;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class ConfirmGradeForItemUseCase(
    ICashSessionRepository cashSessionRepository,
    ISalesLegacyPort salesLegacyPort,
    CartStockReservationService stockReservation,
    ResolveGradeVariantUseCase resolveGradeVariantUseCase)
    : IUseCase<(int tenantId, Guid operatorUserId, Guid lineId, ConfirmGradeForItemInputDto input), CashSessionOutputDto>
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
        var line = session.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
            return Result<CashSessionOutputDto>.Failure("Cart line not found.");

        var resolve = await resolveGradeVariantUseCase.Execute(
            (tenantId, line.ProductId, new ResolveGradeVariantInputDto
            {
                GradeVariantId = input.GradeVariantId,
                OptionIds = input.GradeOptionIds.Count > 0 ? input.GradeOptionIds : null
            }),
            cancellationToken);

        if (resolve.IsFailure)
            return Result<CashSessionOutputDto>.Failure(
                resolve.Error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? "Combinação não cadastrada. Cadastre a combinação com estoque no produto."
                    : resolve.Error);

        if (resolve.Value.AvailableStock < line.Quantity)
            return Result<CashSessionOutputDto>.Failure(
                $"Estoque insuficiente para «{resolve.Value.Label}». Disponível: {resolve.Value.AvailableStock}.");

        var variantId = resolve.Value.VariantId;
        var optionIds = resolve.Value.OptionIds;

        var confirmResult = session.ConfirmGradeForLine(lineId, optionIds, variantId);
        if (confirmResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(confirmResult.Error);

        var confirmedLine = session.Lines.First(l => l.Id == lineId);
        var reserve = await stockReservation.ReserveLineAsync(tenantIdResult.Value, confirmedLine, cancellationToken);
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
