using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Returns.Domain.ValueObjects;

public sealed record GradeSelection
{
    public const int MaxOptionCount = 2;

    public Guid? VariantId { get; }
    public IReadOnlyList<Guid> OptionIds { get; }

    private GradeSelection(Guid? variantId, IReadOnlyList<Guid> optionIds)
    {
        VariantId = variantId;
        OptionIds = optionIds;
    }

    public static Result<GradeSelection?> Create(Guid? variantId, IReadOnlyList<Guid>? optionIds)
    {
        var ids = optionIds?.Where(id => id != Guid.Empty).Distinct().ToList() ?? [];

        if (variantId is null && ids.Count == 0)
            return Result<GradeSelection?>.Success(null);

        if (ids.Count > MaxOptionCount)
            return Result<GradeSelection?>.Failure("Grade selection supports at most 2 dimensions.");

        return Result<GradeSelection?>.Success(new GradeSelection(variantId, ids));
    }

    public bool HasGrade => VariantId is not null || OptionIds.Count > 0;
}
