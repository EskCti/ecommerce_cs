using RetailOps.Core.Notifications.Application.Ports;
using RetailOps.Core.Notifications.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;
using RetailOps.Shared.Kernel.Domain.ValueObjects;

namespace RetailOps.Core.Notifications.Domain.Services;

public sealed class DailyDigestComposer(
    IReceivablesDueTodayQueryPort receivablesPort,
    ILowStockSummaryQueryPort lowStockPort,
    ITenantBillingAlertQueryPort billingPort)
{
    public async Task<Result<DigestContent>> ComposeAsync(TenantId tenantId, CancellationToken ct = default)
    {
        var receivablesResult = await receivablesPort.GetDueTodaySummaryAsync(tenantId, ct);
        if (receivablesResult.IsFailure)
            return Result<DigestContent>.Failure(receivablesResult.Error);

        var lowStockResult = await lowStockPort.GetLowStockSummaryAsync(tenantId, ct);
        if (lowStockResult.IsFailure)
            return Result<DigestContent>.Failure(lowStockResult.Error);

        var billingResult = await billingPort.GetBillingAlertAsync(tenantId, ct);
        if (billingResult.IsFailure)
            return Result<DigestContent>.Failure(billingResult.Error);

        return Result<DigestContent>.Success(new DigestContent
        {
            ReceivablesDueTodayCount = receivablesResult.Value.Count,
            ReceivablesDueTodayTotal = receivablesResult.Value.TotalAmount,
            LowStockProductCount = lowStockResult.Value.Count,
            TopLowStockProductNames = lowStockResult.Value.TopProductNames,
            BillingAlertText = billingResult.Value,
        });
    }

    public Result<MessageTemplate> FormatMessage(DigestContent content, string storeName)
    {
        var lines = new List<string>
        {
            $"*Resumo diário — {storeName}*",
            $"Data: {DateTime.Now:dd/MM/yyyy}",
            "",
        };

        if (content.ReceivablesDueTodayCount > 0)
        {
            lines.Add($"*Contas vencendo hoje:* {content.ReceivablesDueTodayCount}");
            lines.Add($"Total: {content.ReceivablesDueTodayTotal:C}");
            lines.Add("");
        }

        if (content.LowStockProductCount > 0)
        {
            lines.Add($"*Estoque baixo:* {content.LowStockProductCount} produto(s)");
            foreach (var name in content.TopLowStockProductNames.Take(5))
                lines.Add($"• {name}");
            lines.Add("");
        }

        if (!string.IsNullOrWhiteSpace(content.BillingAlertText))
        {
            lines.Add("*Cobrança:*");
            lines.Add(content.BillingAlertText!);
            lines.Add("");
        }

        if (!content.HasAnySection)
            lines.Add("Nenhum alerta operacional para hoje.");

        return MessageTemplate.Create(string.Join('\n', lines));
    }
}
