using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.Entities;

public sealed class GradeVariant : Entity
{
    public const int MaxOptionCount = 2;

    public Guid ProductId { get; private set; }
    public IReadOnlyList<Guid> OptionIds => _optionIds.AsReadOnly();
    public StockQuantity Stock { get; private set; } = null!;

    private readonly List<Guid> _optionIds = [];

    private GradeVariant() { }

    private GradeVariant(Guid productId, IEnumerable<Guid> optionIds, StockQuantity stock)
    {
        ProductId = productId;
        _optionIds.AddRange(optionIds);
        Stock = stock;
    }

    public static Result<GradeVariant> Create(Guid productId, IEnumerable<Guid> optionIds, StockQuantity stock)
    {
        var ids = optionIds.Distinct().ToList();
        if (ids.Count is < 1 or > MaxOptionCount)
            return Result<GradeVariant>.Failure("Grade variant must link 1 or 2 options.");

        if (ids.Any(id => id == Guid.Empty))
            return Result<GradeVariant>.Failure("Grade option id is required.");

        return Result<GradeVariant>.Success(new GradeVariant(productId, ids, stock));
    }

    public static Result<GradeVariant> Reconstitute(
        Guid id,
        Guid productId,
        IEnumerable<Guid> optionIds,
        StockQuantity stock)
    {
        var create = Create(productId, optionIds, stock);
        if (create.IsFailure)
            return create;

        var variant = create.Value;
        variant.SyncIdentity(id);
        return Result<GradeVariant>.Success(variant);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public bool MatchesOptions(IEnumerable<Guid> optionIds)
    {
        var requested = optionIds.OrderBy(x => x).ToList();
        var current = _optionIds.OrderBy(x => x).ToList();
        return requested.SequenceEqual(current);
    }

    public bool ReferencesOption(Guid optionId) => _optionIds.Contains(optionId);

    public Result AdjustStock(StockQuantity newStock)
    {
        Stock = newStock;
        return Result.Success();
    }

    public string BuildLabel(IReadOnlyList<GradeDimension> dimensions)
    {
        var labels = new List<string>();
        foreach (var dimension in dimensions.OrderBy(d => d.Id))
        {
            foreach (var optionId in _optionIds)
            {
                var option = dimension.Options.FirstOrDefault(o => o.Id == optionId);
                if (option is not null)
                {
                    labels.Add(option.Label.Value);
                    break;
                }
            }
        }

        if (labels.Count == 0)
        {
            foreach (var optionId in _optionIds)
            {
                foreach (var dimension in dimensions)
                {
                    var option = dimension.Options.FirstOrDefault(o => o.Id == optionId);
                    if (option is not null)
                    {
                        labels.Add(option.Label.Value);
                        break;
                    }
                }
            }
        }

        return labels.Count == 0 ? string.Empty : string.Join(" / ", labels);
    }
}
