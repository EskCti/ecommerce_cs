using RetailOps.Platform.Core.Domain.Enums;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Platform.Core.Domain.Entities;

public sealed class Contract : Entity
{
    public int LegacyContractId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateOnly SignedDate { get; private set; }

    private Contract(int legacyContractId, string text, DateOnly signedDate)
    {
        LegacyContractId = legacyContractId;
        Text = text;
        SignedDate = signedDate;
    }

    public static Result<Contract> Create(int legacyContractId, string text, DateOnly signedDate)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result<Contract>.Failure("Contract text is required.");

        return Result<Contract>.Success(new Contract(legacyContractId, text.Trim(), signedDate));
    }

    public static Result<Contract> Reconstitute(Guid id, int legacyContractId, string text, DateOnly signedDate)
    {
        var contract = Create(legacyContractId, text, signedDate);
        if (contract.IsFailure) return contract;
        contract.Value.Id = id;
        return contract;
    }

    public Result Update(string text, DateOnly signedDate)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Failure("Contract text is required.");

        Text = text.Trim();
        SignedDate = signedDate;
        return Result.Success();
    }
}
