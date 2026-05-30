using RetailOps.Core.Finance.Application.DTOs;
using RetailOps.Core.Finance.Domain.Entities;
using RetailOps.Core.Finance.Domain.Repositories;
using RetailOps.Core.Finance.Domain.Services;
using RetailOps.Core.Finance.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Application.UseCases;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Finance.Application.UseCases;

public sealed class CreatePayableUseCase(IPayableRepository payableRepository)
    : IUseCase<(int tenantId, CreatePayableInputDto input), PayableOutputDto>
{
    public async Task<Result<PayableOutputDto>> Execute(
        (int tenantId, CreatePayableInputDto input) request)
    {
        var (tenantId, input) = request;
        var tenantResult = TenantId.Create(tenantId);
        if (tenantResult.IsFailure)
            return Result<PayableOutputDto>.Failure(tenantResult.Error);

        var amountResult = Money.Create(input.Amount);
        if (amountResult.IsFailure)
            return Result<PayableOutputDto>.Failure(amountResult.Error);

        var dueDateResult = DueDate.Create(input.DueDate);
        if (dueDateResult.IsFailure)
            return Result<PayableOutputDto>.Failure(dueDateResult.Error);

        Recurrence? recurrence = null;
        if (input.RecurrenceDays is not null)
        {
            var recurrenceResult = Recurrence.Create(input.RecurrenceDays.Value);
            if (recurrenceResult.IsFailure)
                return Result<PayableOutputDto>.Failure(recurrenceResult.Error);
            recurrence = recurrenceResult.Value;
        }

        var payableResult = Payable.CreateExpense(
            tenantResult.Value,
            input.Description,
            amountResult.Value,
            dueDateResult.Value,
            recurrence,
            input.PersonLegacyId);

        if (payableResult.IsFailure)
            return Result<PayableOutputDto>.Failure(payableResult.Error);

        var saveResult = await payableRepository.Save(payableResult.Value);
        if (saveResult.IsFailure)
            return Result<PayableOutputDto>.Failure(saveResult.Error);

        return Result<PayableOutputDto>.Success(PayableOutputDto.FromDomain(payableResult.Value));
    }
}

public sealed class UpdatePayableUseCase(IPayableRepository payableRepository)
    : IUseCase<(int tenantId, Guid id, UpdatePayableInputDto input), PayableOutputDto>
{
    public async Task<Result<PayableOutputDto>> Execute(
        (int tenantId, Guid id, UpdatePayableInputDto input) request)
    {
        var (tenantId, id, input) = request;
        var payableResult = await payableRepository.GetById(id);
        if (payableResult.IsFailure)
            return Result<PayableOutputDto>.Failure(payableResult.Error);

        if (payableResult.Value.TenantId.Value != tenantId)
            return Result<PayableOutputDto>.Failure("Payable does not belong to this tenant.");

        var amountResult = Money.Create(input.Amount);
        if (amountResult.IsFailure)
            return Result<PayableOutputDto>.Failure(amountResult.Error);

        var dueDateResult = DueDate.Create(input.DueDate);
        if (dueDateResult.IsFailure)
            return Result<PayableOutputDto>.Failure(dueDateResult.Error);

        Recurrence? recurrence = null;
        if (input.RecurrenceDays is not null)
        {
            var recurrenceResult = Recurrence.Create(input.RecurrenceDays.Value);
            if (recurrenceResult.IsFailure)
                return Result<PayableOutputDto>.Failure(recurrenceResult.Error);
            recurrence = recurrenceResult.Value;
        }

        var updateResult = payableResult.Value.Update(
            input.Description,
            amountResult.Value,
            dueDateResult.Value,
            recurrence,
            input.PersonLegacyId);

        if (updateResult.IsFailure)
            return Result<PayableOutputDto>.Failure(updateResult.Error);

        var saveResult = await payableRepository.Save(payableResult.Value);
        if (saveResult.IsFailure)
            return Result<PayableOutputDto>.Failure(saveResult.Error);

        return Result<PayableOutputDto>.Success(PayableOutputDto.FromDomain(payableResult.Value));
    }
}

public sealed class DeletePayableUseCase(IPayableRepository payableRepository)
    : IUseCase<(int tenantId, Guid id)>
{
    public async Task<Result> Execute((int tenantId, Guid id) request)
    {
        var (tenantId, id) = request;
        var payableResult = await payableRepository.GetById(id);
        if (payableResult.IsFailure)
            return Result.Failure(payableResult.Error);

        if (payableResult.Value.TenantId.Value != tenantId)
            return Result.Failure("Payable does not belong to this tenant.");

        if (payableResult.Value.Type != AccountType.Expense)
            return Result.Failure("Only expense payables can be deleted.");

        return await payableRepository.Delete(id);
    }
}

public sealed class GetPayableUseCase(IPayableRepository payableRepository)
    : IUseCase<(int tenantId, Guid id), PayableOutputDto>
{
    public async Task<Result<PayableOutputDto>> Execute((int tenantId, Guid id) request)
    {
        var (tenantId, id) = request;
        var payableResult = await payableRepository.GetById(id);
        if (payableResult.IsFailure)
            return Result<PayableOutputDto>.Failure(payableResult.Error);

        if (payableResult.Value.TenantId.Value != tenantId)
            return Result<PayableOutputDto>.Failure("Payable does not belong to this tenant.");

        return Result<PayableOutputDto>.Success(PayableOutputDto.FromDomain(payableResult.Value));
    }
}

public sealed class SettlePayableUseCase(
    IPayableRepository payableRepository,
    PayableSettlementService settlementService)
    : IUseCase<(int tenantId, Guid id, SettleAccountInputDto input), PayableOutputDto>
{
    public async Task<Result<PayableOutputDto>> Execute(
        (int tenantId, Guid id, SettleAccountInputDto input) request)
    {
        var (tenantId, id, input) = request;
        var payableResult = await payableRepository.GetById(id);
        if (payableResult.IsFailure)
            return Result<PayableOutputDto>.Failure(payableResult.Error);

        if (payableResult.Value.TenantId.Value != tenantId)
            return Result<PayableOutputDto>.Failure("Payable does not belong to this tenant.");

        var settleResult = settlementService.Settle(payableResult.Value, input.SettlementDate);
        if (settleResult.IsFailure)
            return Result<PayableOutputDto>.Failure(settleResult.Error);

        var saveResult = await payableRepository.Save(payableResult.Value);
        if (saveResult.IsFailure)
            return Result<PayableOutputDto>.Failure(saveResult.Error);

        return Result<PayableOutputDto>.Success(PayableOutputDto.FromDomain(payableResult.Value));
    }
}

public sealed class AddPayableAttachmentUseCase(IPayableRepository payableRepository)
    : IUseCase<(int tenantId, Guid id, AddFinanceAttachmentInputDto input), FinanceAttachmentOutputDto>
{
    public async Task<Result<FinanceAttachmentOutputDto>> Execute(
        (int tenantId, Guid id, AddFinanceAttachmentInputDto input) request)
    {
        var (tenantId, id, input) = request;
        var tenantResult = TenantId.Create(tenantId);
        if (tenantResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(tenantResult.Error);

        var payableResult = await payableRepository.GetById(id);
        if (payableResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(payableResult.Error);

        if (payableResult.Value.TenantId.Value != tenantId)
            return Result<FinanceAttachmentOutputDto>.Failure("Payable does not belong to this tenant.");

        var metaResult = AttachmentMeta.Create(input.Name, input.Path);
        if (metaResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(metaResult.Error);

        var attachmentResult = FinanceAttachment.Create(tenantResult.Value, id, metaResult.Value);
        if (attachmentResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(attachmentResult.Error);

        var addResult = payableResult.Value.AddAttachment(attachmentResult.Value);
        if (addResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(addResult.Error);

        var saveResult = await payableRepository.Save(payableResult.Value);
        if (saveResult.IsFailure)
            return Result<FinanceAttachmentOutputDto>.Failure(saveResult.Error);

        return Result<FinanceAttachmentOutputDto>.Success(
            FinanceAttachmentOutputDto.FromDomain(attachmentResult.Value));
    }
}
