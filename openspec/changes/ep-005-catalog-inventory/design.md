## Context

**RetailOps** — BC **Catalog & Inventory** (Core). Épico grande (G) com 4 user stories no backlog. Bloqueia EP-006 Sales (PDV scan, reserva estoque, grades no carrinho).

Legado: `produtos`, `categorias`, `cat_grade`, `itens_grade`, `entradas`, `saidas`, `detalhes_grade`, `produtos/comprar.php`.

Referências: `ddd-tactical-model.md`, `acl-design.md` (BC Catalog), `migration-strategy.md` (parallel run Catalog/Sales).

## Goals / Non-Goals

**Goals:**

- Paridade RF-030–RF-034, RF-040–RF-042, RF-044
- Regras RN-020 (preço aberto), RN-021 (alerta), RN-022 (lucro), RN-030/031 (movimentações)
- `FindByBarcode` OHS para PDV
- `IStockLegacyPort.AdjustStock` / `ReserveStock` para Sales
- Parallel run: leitura/compare estoque vs PHP pilot tenant
- UI Vue completa + Android produtos (US-050 template)

**Non-Goals:**

- PDV carrinho/finalização (EP-006)
- Contas a pagar completas (EP-007 — apenas evento `ProductPurchased`)
- B2C vitrine online
- Geração automática código RF-031 como microserviço separado (incluir como use case simples)

## Decisions

### 1. Módulo Catalog

**Decisão:** `RetailOps.Catalog.*`; tenant-scoped; referência opcional `SupplierId` do CRM.

### 2. Product aggregate

**Decisão:** `Product` AR com árvore opcional `GradeDimension[] → GradeOption[]`; flags `OpenPrice`, `ActiveStatus`; métodos `UpdatePricing()`, `AdjustStock()`, `ConfigureGrades()`.

**Barcode:** único por tenant; `GenerateBarcodeUseCase` (RF-031).

**Foto:** whitelist extensões; MVP path/URL string.

### 3. Category aggregate

**Decisão:** `Category` AR separado; produto referencia `CategoryId`.

### 4. Stock movements

**Decisão:** `StockMovement` AR audit log; tipos Entry/Exit/Purchase; `PurchaseStockUseCase` publica `ProductPurchased` com dados para Finance ACL.

**Grade movements:** `GradeMovementDetail` com `MovementType` enum alinhado RN-031.

### 5. Open Host Service (OHS)

**Decisão:** Interface application `IProductCatalogService`:
- `FindByBarcode(tenant, barcode)` → produto + grades + estoque disponível
- `ReserveStock` / `ReleaseStock` chamado por Sales BC

Implementação delega a ACL durante parallel run.

### 6. Low stock

**Decisão:** `LowStockPolicy` + `ListLowStockProductsQuery`; dashboard Vue card/list.

### 7. Parallel run

**Decisão:** Endpoint admin ou job compara `StockQuantity` RetailOps vs legado para pilot tenants; log divergência > threshold.

### 8. API routes

- `/api/catalog/products`, `/api/catalog/categories`
- `/api/catalog/products/{id}/grades`
- `/api/catalog/stock/movements`, `/api/catalog/stock/purchase`
- `/api/catalog/stock/low`
- `/api/catalog/products/by-barcode/{code}` (OHS HTTP)

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Grade estoque dual-write complexo | ACL `LegacyGradeDetailMapper`; testes integração |
| Parallel run estoque divergente | Compare job + alertas |
| ProductPurchased sem Finance | Event + interface stub documentada |
| Upload foto | MVP path; storage EP posterior |

## Migration Plan

1. Products/categories → grades → movements → OHS → UI → parallel run validation
2. Pilot flag Catalog BC
3. EP-006 PDV consome OHS

**Rollback:** flag off; PHP produtos/estoque ativos.

## Open Questions

- Reserva estoque pessimista vs otimista no PDV? **Reserva na confirmação item carrinho (EP-006)**
- Compra gera payable inline ou só evento? **Evento ProductPurchased; Finance EP-007 persiste payable**
