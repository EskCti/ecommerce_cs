namespace RetailOps.Core.Notifications.Domain.ValueObjects;

public sealed record DigestContent
{
    public int ReceivablesDueTodayCount { get; init; }
    public decimal ReceivablesDueTodayTotal { get; init; }
    public int LowStockProductCount { get; init; }
    public IReadOnlyList<string> TopLowStockProductNames { get; init; } = [];
    public string? BillingAlertText { get; init; }

    public bool HasAnySection =>
        ReceivablesDueTodayCount > 0
        || LowStockProductCount > 0
        || !string.IsNullOrWhiteSpace(BillingAlertText);
}
