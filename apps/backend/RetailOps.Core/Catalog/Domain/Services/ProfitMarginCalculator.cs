using RetailOps.Core.Catalog.Domain.ValueObjects;
using RetailOps.Shared.Kernel.Domain.Results;

namespace RetailOps.Core.Catalog.Domain.Services;

public static class ProfitMarginCalculator
{
    public static Result<ProfitMargin> Calculate(SalePrice salePrice, CostPrice costPrice)
    {
        if (costPrice.Value <= 0)
            return ProfitMargin.Create(0);

        var margin = (salePrice.Value - costPrice.Value) / costPrice.Value * 100m;
        return ProfitMargin.Create(Math.Round(margin, 2));
    }
}
