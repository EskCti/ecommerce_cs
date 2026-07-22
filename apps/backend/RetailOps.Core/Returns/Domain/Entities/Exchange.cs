using RetailOps.Core.Crm.Domain.ValueObjects;
using RetailOps.Core.Returns.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Base;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Returns.Domain.Entities;

public sealed class Exchange : Entity
{
    public TenantId TenantId { get; private set; }
    public CustomerId CustomerId { get; private set; } = null!;
    public Guid ProductInId { get; private set; }
    public Guid ProductOutId { get; private set; }
    public GradeSelection? GradeIn { get; private set; }
    public GradeSelection? GradeOut { get; private set; }
    public ExchangeQuantity Quantity { get; private set; } = null!;
    public Guid OperatorUserId { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private Exchange() { }

    public static Result<Exchange> Register(
        TenantId tenantId,
        CustomerId customerId,
        Guid productInId,
        Guid productOutId,
        GradeSelection? gradeIn,
        GradeSelection? gradeOut,
        Guid operatorUserId,
        int availableOutboundStock)
    {
        if (productInId == Guid.Empty)
            return Result<Exchange>.Failure("Product in id is required.");

        if (productOutId == Guid.Empty)
            return Result<Exchange>.Failure("Product out id is required.");

        if (productInId == productOutId)
            return Result<Exchange>.Failure("Product in and product out must be different.");

        if (operatorUserId == Guid.Empty)
            return Result<Exchange>.Failure("Operator user id is required.");

        var quantityResult = ExchangeQuantity.One();
        if (quantityResult.IsFailure)
            return Result<Exchange>.Failure(quantityResult.Error);

        if (availableOutboundStock < quantityResult.Value.Value)
            return Result<Exchange>.Failure("Insufficient stock on outbound product.");

        return Result<Exchange>.Success(new Exchange
        {
            TenantId = tenantId,
            CustomerId = customerId,
            ProductInId = productInId,
            ProductOutId = productOutId,
            GradeIn = gradeIn,
            GradeOut = gradeOut,
            Quantity = quantityResult.Value,
            OperatorUserId = operatorUserId,
            RegisteredAt = DateTime.UtcNow
        });
    }

    public static Result<Exchange> Reconstitute(
        Guid id,
        TenantId tenantId,
        CustomerId customerId,
        Guid productInId,
        Guid productOutId,
        GradeSelection? gradeIn,
        GradeSelection? gradeOut,
        ExchangeQuantity quantity,
        Guid operatorUserId,
        DateTime registeredAt)
    {
        var exchange = new Exchange
        {
            Id = id,
            TenantId = tenantId,
            CustomerId = customerId,
            ProductInId = productInId,
            ProductOutId = productOutId,
            GradeIn = gradeIn,
            GradeOut = gradeOut,
            Quantity = quantity,
            OperatorUserId = operatorUserId,
            RegisteredAt = registeredAt
        };

        return Result<Exchange>.Success(exchange);
    }

    internal void SyncIdentity(Guid id) => Id = id;
}
