## Why

O **BC Catalog & Inventory** (Core) é pré-requisito do MVP PDV (EP-006): operadores escaneiam produtos por código de barras, reservam estoque e vendem com ou sem grade. Sem catálogo, categorias, grades, movimentações e alerta de estoque baixo, o parallel run contra o PHP falha e o PDV não tem `FindByBarcode` nem políticas de estoque (RN-020–RN-022, RN-030–RN-031).

## What Changes

- Novo módulo `RetailOps.Catalog` (Core, Application, Infrastructure)
- Agregados `Product`, `Category`, `StockMovement`; entidades `GradeDimension`, `GradeOption`, `GradeMovementDetail`
- VOs: `Barcode`, `SalePrice`, `CostPrice`, `StockQuantity`, `ProfitMargin`, `StockAlertLevel`, `MovementType`
- Serviços: `ProfitMarginCalculator`, `StockAdjustmentPolicy`, `LowStockPolicy`, **`ProductCatalogQuery`** (OHS para Sales)
- Casos de uso: CRUD produto/categoria, configurar grades, entrada/saída manual, compra estoque (`ProductPurchased` event)
- ACL: `LegacyProductAdapter`, `LegacyStockMovementMapper`, `IProductCatalogLegacyPort`, `IStockLegacyPort`
- API tenant-scoped + parallel run read/compare estoque
- Frontend Vue: produtos, categorias, editor de grades, movimentação estoque, dashboard estoque baixo
- Mobile Android: listagem/consulta produto (template full-stack US-050)
- Testes ≥95% + E2E + validação parallel run estoque

## Capabilities

### New Capabilities

- `product-category-management`: CRUD produtos e categorias, código único/tenant, preço aberto, foto whitelist
- `product-grades`: Dimensões Cor/Tamanho, opções com estoque por combinação
- `stock-movements`: Entrada/saída manual, compra com evento `ProductPurchased`
- `low-stock-alerts`: Política RN-021, query e dashboard produtos abaixo do mínimo
- `catalog-open-host-service`: Porta `FindByBarcode` e reserva estoque para Sales BC
- `catalog-web-ui`: UI Vue admin catálogo, grades, estoque
- `catalog-mobile-products`: Telas Android consulta/listagem produto

### Modified Capabilities

_(nenhuma — Finance EP-007 consumirá `ProductPurchased` via evento, sem delta de spec existente neste change)_

## Impact

- **Backend**: módulo Catalog + ACL parallel run crítico para estoque
- **Sales EP-006**: depende de `IProductCatalogLegacyPort` / API catalog
- **Finance EP-007**: listener `ProductPurchased` (stub/interface neste épico)
- **Dependências**: EP-000, EP-001, EP-003 (Settings); EP-004 CRM opcional (fornecedor em produto)
- **Release**: 1 (MVP junto com PDV)
