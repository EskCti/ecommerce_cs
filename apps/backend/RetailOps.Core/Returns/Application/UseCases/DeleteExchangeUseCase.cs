using RetailOps.Core.Returns.Application.DTOs;
using RetailOps.Core.Returns.Domain.Repositories;
using RetailOps.Core.Returns.Domain.Services;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Returns.Application.UseCases;

public sealed class DeleteExchangeUseCase(
    IExchangeRepository exchangeRepository,
    ExchangeStockPolicy exchangeStockPolicy)
    : IUseCase<(int tenantId, Guid exchangeId), ExchangeOutputDto>
{
    public async Task<Result<ExchangeOutputDto>> Execute(
        (int tenantId, Guid exchangeId) request,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, exchangeId) = request;

        var tenantIdResult = TenantId.Create(tenantId);
        if (tenantIdResult.IsFailure)
            return Result<ExchangeOutputDto>.Failure(tenantIdResult.Error);

        var getResult = await exchangeRepository.GetById(tenantIdResult.Value, exchangeId, cancellationToken);
        if (getResult.IsFailure)
            return Result<ExchangeOutputDto>.Failure(getResult.Error);

        var exchange = getResult.Value;

        var reverseStock = await exchangeStockPolicy.ApplyDeleteReverseAsync(
            tenantIdResult.Value,
            exchange.ProductInId,
            exchange.GradeIn,
            exchange.ProductOutId,
            exchange.GradeOut,
            cancellationToken);
        if (reverseStock.IsFailure)
            return Result<ExchangeOutputDto>.Failure(reverseStock.Error);

        var deleteResult = await exchangeRepository.Delete(tenantIdResult.Value, exchangeId, cancellationToken);
        if (deleteResult.IsFailure)
        {
            await exchangeStockPolicy.ApplyRegisterAsync(
                tenantIdResult.Value,
                exchange.ProductInId,
                exchange.GradeIn,
                exchange.ProductOutId,
                exchange.GradeOut,
                cancellationToken);
            return Result<ExchangeOutputDto>.Failure(deleteResult.Error);
        }

        return Result<ExchangeOutputDto>.Success(ExchangeOutputDto.FromDomain(exchange));
    }
}
