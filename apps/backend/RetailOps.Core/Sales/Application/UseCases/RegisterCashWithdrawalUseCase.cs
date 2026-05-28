using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Application.Ports;
using RetailOps.Core.Sales.Domain.Entities;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class RegisterCashWithdrawalUseCase(
    ICashSessionRepository cashSessionRepository,
    ISalesLegacyPort salesLegacyPort) : IUseCase<(int tenantId, Guid operatorUserId, RegisterCashWithdrawalInputDto input), CashSessionOutputDto>
{
    public async Task<Result<CashSessionOutputDto>> Execute(
        (int tenantId, Guid operatorUserId, RegisterCashWithdrawalInputDto input) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, operatorUserId, input) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(tenantIdResult.Error);

        var sessionResult = await cashSessionRepository.GetOpenByOperator(tenantIdResult.Value, operatorUserId);
        if (sessionResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(sessionResult.Error);

        if (sessionResult.Value is null)
            return Result<CashSessionOutputDto>.Failure("No open cash session found.");

        var withdrawalResult = CashWithdrawal.Create(input.Amount);
        if (withdrawalResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(withdrawalResult.Error);

        var session = sessionResult.Value;
        var registerResult = session.RegisterWithdrawal(withdrawalResult.Value);
        if (registerResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(registerResult.Error);

        var saveLegacy = await salesLegacyPort.SaveSession(session, cancellationToken);
        if (saveLegacy.IsFailure)
            return Result<CashSessionOutputDto>.Failure(saveLegacy.Error);

        var saveResult = await cashSessionRepository.Save(session);
        if (saveResult.IsFailure)
            return Result<CashSessionOutputDto>.Failure(saveResult.Error);

        return Result<CashSessionOutputDto>.Success(CashSessionOutputDto.FromDomain(session));
    }
}
