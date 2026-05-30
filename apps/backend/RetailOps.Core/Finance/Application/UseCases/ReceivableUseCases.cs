using RetailOps.Core.Finance.Application.DTOs;
using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.Repositories;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Application.UseCases;

public sealed class CreateReceivableUseCase(IReceivableRepository receivableRepository)
    : IUseCase<(int tenantId, CreateReceivableInputDto input), ReceivableOutputDto>
{
    public async Task<Result<ReceivableOutputDto>> Execute(
        (int tenantId, CreateReceivableInputDto input) request)
    {
        var (tenantId, input) = request;
        var tenantResult = TenantId.Create(tenantId);
        if (tenantResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(tenantResult.Error);

        var amountResult = Money.Create(input.Amount);
        if (amountResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(amountResult.Error);

        var dueDateResult = DueDate.Create(input.DueDate);
        if (dueDateResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(dueDateResult.Error);

        var receivableResult = Receivable.CreateManual(
            tenantResult.Value,
            input.Description,
            amountResult.Value,
            dueDateResult.Value,
            input.PersonLegacyId);

        if (receivableResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(receivableResult.Error);

        var saveResult = await receivableRepository.Save(receivableResult.Value);
        if (saveResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(saveResult.Error);

        return Result<ReceivableOutputDto>.Success(ReceivableOutputDto.FromDomain(receivableResult.Value));
    }
}

public sealed class UpdateReceivableUseCase(IReceivableRepository receivableRepository)
    : IUseCase<(int tenantId, Guid id, UpdateReceivableInputDto input), ReceivableOutputDto>
{
    public async Task<Result<ReceivableOutputDto>> Execute(
        (int tenantId, Guid id, UpdateReceivableInputDto input) request)
    {
        var (tenantId, id, input) = request;
        var receivableResult = await receivableRepository.GetById(id);
        if (receivableResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(receivableResult.Error);

        if (receivableResult.Value.TenantId.Value != tenantId)
            return Result<ReceivableOutputDto>.Failure("Receivable does not belong to this tenant.");

        var amountResult = Money.Create(input.Amount);
        if (amountResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(amountResult.Error);

        var dueDateResult = DueDate.Create(input.DueDate);
        if (dueDateResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(dueDateResult.Error);

        var updateResult = receivableResult.Value.Update(
            input.Description,
            amountResult.Value,
            dueDateResult.Value,
            input.PersonLegacyId);

        if (updateResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(updateResult.Error);

        var saveResult = await receivableRepository.Save(receivableResult.Value);
        if (saveResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(saveResult.Error);

        return Result<ReceivableOutputDto>.Success(ReceivableOutputDto.FromDomain(receivableResult.Value));
    }
}

public sealed class DeleteReceivableUseCase(IReceivableRepository receivableRepository)
    : IUseCase<(int tenantId, Guid id)>
{
    public async Task<Result> Execute((int tenantId, Guid id) request)
    {
        var (tenantId, id) = request;
        var receivableResult = await receivableRepository.GetById(id);
        if (receivableResult.IsFailure)
            return Result.Failure(receivableResult.Error);

        if (receivableResult.Value.TenantId.Value != tenantId)
            return Result.Failure("Receivable does not belong to this tenant.");

        if (receivableResult.Value.SaleId is not null)
            return Result.Failure("Cannot delete sale-linked receivable.");

        return await receivableRepository.Delete(id);
    }
}

public sealed class GetReceivableUseCase(IReceivableRepository receivableRepository)
    : IUseCase<(int tenantId, Guid id), ReceivableOutputDto>
{
    public async Task<Result<ReceivableOutputDto>> Execute((int tenantId, Guid id) request)
    {
        var (tenantId, id) = request;
        var receivableResult = await receivableRepository.GetById(id);
        if (receivableResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(receivableResult.Error);

        if (receivableResult.Value.TenantId.Value != tenantId)
            return Result<ReceivableOutputDto>.Failure("Receivable does not belong to this tenant.");

        return Result<ReceivableOutputDto>.Success(ReceivableOutputDto.FromDomain(receivableResult.Value));
    }
}

public sealed class SettleReceivableUseCase(
    IReceivableRepository receivableRepository,
    Domain.Services.ReceivableSettlementService settlementService)
    : IUseCase<(int tenantId, Guid id, SettleAccountInputDto input), ReceivableOutputDto>
{
    public async Task<Result<ReceivableOutputDto>> Execute(
        (int tenantId, Guid id, SettleAccountInputDto input) request)
    {
        var (tenantId, id, input) = request;
        var receivableResult = await receivableRepository.GetById(id);
        if (receivableResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(receivableResult.Error);

        if (receivableResult.Value.TenantId.Value != tenantId)
            return Result<ReceivableOutputDto>.Failure("Receivable does not belong to this tenant.");

        var settleResult = settlementService.Settle(receivableResult.Value, input.SettlementDate);
        if (settleResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(settleResult.Error);

        var saveResult = await receivableRepository.Save(receivableResult.Value);
        if (saveResult.IsFailure)
            return Result<ReceivableOutputDto>.Failure(saveResult.Error);

        return Result<ReceivableOutputDto>.Success(ReceivableOutputDto.FromDomain(receivableResult.Value));
    }
}

public sealed class AddReceivableAttachmentUseCase(IReceivableRepository receivableRepository)
    : IUseCase<(int tenantId, Guid id, AddFinanceAttachmentInputDto input), FinanceAttachmentOutputDto>
{
    public async Task<Result<FinanceAttachmentOutputDto>> Execute(
        (int tenantId, Guid id, AddFinanceAttachmentInputDto input) request)
    {
        var (tenantId, id, input) = request;
        var tenantResult = TenantId.Create(tenantId);
        if (tenantResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(tenantResult.Error);

        var receivableResult = await receivableRepository.GetById(id);
        if (receivableResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(receivableResult.Error);

        if (receivableResult.Value.TenantId.Value != tenantId)
            return Result<FinanceAttachmentOutputDto>.Failure("Receivable does not belong to this tenant.");

        var metaResult = AttachmentMeta.Create(input.Name, input.Path);
        if (metaResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(metaResult.Error);

        var attachmentResult = FinanceAttachment.Create(tenantResult.Value, id, metaResult.Value);
        if (attachmentResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(attachmentResult.Error);

        var addResult = receivableResult.Value.AddAttachment(attachmentResult.Value);
        if (addResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(addResult.Error);

        var saveResult = await receivableRepository.Save(receivableResult.Value);
        if (saveResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(saveResult.Error);

        return Result<FinanceAttachmentOutputDto>.Success(
            FinanceAttachmentOutputDto.FromDomain(attachmentResult.Value));
    }
}
