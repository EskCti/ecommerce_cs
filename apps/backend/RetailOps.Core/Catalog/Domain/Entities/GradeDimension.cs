using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Core.Catalog.Domain.ValueObjects;

namespace RetailOps.Core.Catalog.Domain.Entities;

public sealed class GradeDimension : Entity
{
    public Guid ProductId { get; private set; }
    public ProductName Name { get; private set; } = null!;

    private readonly List<GradeOption> _options = [];
    public IReadOnlyList<GradeOption> Options => _options.AsReadOnly();

    private GradeDimension() { }

    private GradeDimension(Guid productId, ProductName name)
    {
        ProductId = productId;
        Name = name;
    }

    public static Result<GradeDimension> Create(Guid productId, ProductName name) =>
        Result<GradeDimension>.Success(new GradeDimension(productId, name));

    public static Result<GradeDimension> Reconstitute(
        Guid id,
        Guid productId,
        ProductName name,
        IEnumerable<GradeOption>? options = null)
    {
        var dimension = new GradeDimension(productId, name) { Id = id };

        if (options is not null)
            dimension._options.AddRange(options);

        return Result<GradeDimension>.Success(dimension);
    }

    internal void SyncIdentity(Guid id) => Id = id;

    public Result<GradeOption> AddOption(ProductName label, StockQuantity stock)
    {
        var optionResult = GradeOption.Create(Id, label, stock);
        if (optionResult.IsFailure)
            return optionResult;

        _options.Add(optionResult.Value);
        return optionResult;
    }

    public Result RemoveOption(Guid optionId)
    {
        var option = _options.FirstOrDefault(o => o.Id == optionId);
        if (option is null)
            return Result.Failure("Grade option not found.");

        _options.Remove(option);
        return Result.Success();
    }

    internal void SetOptions(IEnumerable<GradeOption> options)
    {
        _options.Clear();
        _options.AddRange(options);
    }
}
