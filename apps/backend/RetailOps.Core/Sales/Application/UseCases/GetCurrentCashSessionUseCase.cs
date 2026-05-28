using RetailOps.Core.Sales.Application.DTOs;
using RetailOps.Core.Sales.Domain.Repositories;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Sales.Application.UseCases;

public sealed class GetCurrentCashSessionUseCase(
    ICashSessionRepository cashSessionRepository) : IUseCase<(int tenantId, Guid operatorUserId), CashSessionOutputDto?>
{
    public async Task<Result<CashSessionOutputDto?>> Execute(
        (int tenantId, Guid operatorUserId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, operatorUserId) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<CashSessionOutputDto?>.Failure(tenantIdResult.Error);

        var sessionResult = await cashSessionRepository.GetOpenByOperator(tenantIdResult.Value, operatorUserId);
        if (sessionResult.IsFailure)
            return Result<CashSessionOutputDto?>.Failure(sessionResult.Error);

        if (sessionResult.Value is null)
            return Result<CashSessionOutputDto?>.Success(null);

        return Result<CashSessionOutputDto?>.Success(CashSessionOutputDto.FromDomain(sessionResult.Value));
    }
}
