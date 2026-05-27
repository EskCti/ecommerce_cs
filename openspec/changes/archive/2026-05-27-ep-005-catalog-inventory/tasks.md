# Tasks — ep-005-catalog-inventory (EP-005 Catalog & Inventory)

Referência: `docs/planning/loja-php/backlog.md` · US-050, US-051, US-052, US-053

## 1. Module setup (US-050)

- [x] 1.1 `infra:setup` Módulo Catalog (~1h)
  - **Agent:** `Config New Module (C#)`
  - **Prompt:** "Crie RetailOps.Catalog: Core, Application, Infrastructure; IProductRepository, ICategoryRepository, IStockMovementRepository."
  - **Spec:** `product-category-management`

## 2. Domain — Product & Category (US-050)

- [x] 2.1 `domain:vo` Catalog VOs (~2h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "Barcode unique/tenant, SalePrice, CostPrice, StockQuantity, ProfitMargin, StockAlertLevel, ProductName; OpenPrice flag RN-020."
  - **Spec:** `product-category-management`

- [x] 2.2 `domain:entity` Product, Category (~3h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Product AR: pricing, stock, category, photo path, active; Category AR; ProfitMarginCalculator RN-022."
  - **Spec:** `product-category-management`

- [x] 2.3 `domain:repository` IProductRepository, ICategoryRepository (~1h)
  - **Agent:** `Core Repository (C#)`
  - **Prompt:** "Contratos FindByBarcode, ExistsBarcode, List paginado, Save, Delete."
  - **Spec:** `product-category-management`, `catalog-open-host-service`

- [x] 2.4 `domain:service` ProfitMarginCalculator (~1h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Calcula lucro % na compra/atualização preços RN-022."
  - **Spec:** `product-category-management`

## 3. Application — Product CRUD (US-050)

- [x] 3.1 `app:usecase` CreateProduct, UpdateProduct, GenerateBarcode (~4h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "CreateProduct, UpdateProduct, DeactivateProduct, GenerateBarcodeUseCase RF-031; foto whitelist."
  - **Spec:** `product-category-management`

- [x] 3.2 `app:usecase` Category CRUD (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "CreateCategory, UpdateCategory, ListCategories RF-032."
  - **Spec:** `product-category-management`

- [x] 3.3 `app:query` ListProductsQuery (~1h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ListProductsQuery paginada filtros barcode, nome, categoria, ativo."
  - **Spec:** `product-category-management`

## 4. Infrastructure — Product ACL (US-050)

- [x] 4.1 `infra:persistence` ProductEfRepository + LegacyProductAdapter (~4h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyProductMapper, LegacyProductAdapter, IProductCatalogLegacyPort; parallel run read."
  - **Spec:** `product-category-management`, `catalog-open-host-service`

## 5. API — Products & Categories (US-050)

- [x] 5.1 `interface:controller` CatalogProductsController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "CRUD /api/catalog/products; GET by-barcode; generate-barcode; tenant scope."
  - **Spec:** `product-category-management`, `catalog-open-host-service`

- [x] 5.2 `interface:controller` CatalogCategoriesController (~1h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "CRUD /api/catalog/categories."
  - **Spec:** `product-category-management`

## 6. Frontend full-stack — Product (US-050)

- [x] 6.1 `interface:entity` Product Vue (~1h)
  - **Agent:** `Frontend Entity (Vue)`
  - **Prompt:** "Entidade Product TypeScript pura Result<T>; OpenPrice, barcode validation."

- [x] 6.2 `interface:usecase` + `interface:repository` Product (~3h)
  - **Agent:** `Frontend UseCase (Vue)` + `Frontend Repository (Vue)`
  - **Prompt:** "CRUD + ListProducts + GenerateBarcode; /api/catalog/products."

- [x] 6.3 `interface:page` + `interface:form-web` Produtos (~4h)
  - **Agent:** `Frontend Page (Vue)` + `Frontend Form (Vue)`
  - **Prompt:** "/catalog/products DataTable + form RF-030; upload foto path whitelist."
  - **Spec:** `catalog-web-ui`

- [x] 6.4 `interface:form-web` Categorias (~2h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "Gestão categorias ativo/inativo RF-032."
  - **Spec:** `catalog-web-ui`

## 7. Mobile — Product (US-050)

- [x] 7.1 `interface:mobile-entity` + usecase + repository Product Android (~4h)
  - **Agent:** `Mobile Entity (Android)` + `Mobile UseCase (Android)` + `Mobile Repository (Android)`
  - **Prompt:** "ListProducts + FindByBarcode; Retrofit catalog API."

- [x] 7.2 `interface:mobile` ProductListScreen + detail (~3h)
  - **Agent:** `Mobile Screen (Android)`
  - **Prompt:** "LazyColumn produtos + tela detalhe barcode scan."
  - **Spec:** `catalog-mobile-products`

## 8. Grades (US-051)

- [x] 8.1 `domain:entity` GradeDimension, GradeOption, GradeMovementDetail (~3h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Grade tree under Product; GradeMovementDetail MovementType enum RN-031."
  - **Spec:** `product-grades`

- [x] 8.2 `app:usecase` ConfigureProductGradeUseCase (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "AddDimension, AddOption, RemoveGrade, AdjustGradeStock RF-033/034."
  - **Spec:** `product-grades`

- [x] 8.3 `infra:persistence` LegacyGradeDetailMapper (~2h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyGradeDetailMapper detalhes_grade polimórfico."
  - **Spec:** `product-grades`

- [x] 8.4 `interface:controller` + Vue grade editor (~4h)
  - **Agent:** `Backend Controller (C#)` + `Frontend Form (Vue)`
  - **Prompt:** "API /products/{id}/grades + UI editor dimensões/opções estoque."
  - **Spec:** `product-grades`, `catalog-web-ui`

## 9. Stock movements (US-052)

- [x] 9.1 `domain:entity` StockMovement (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "StockMovement AR audit; MovementType Entry|Exit|Purchase."
  - **Spec:** `stock-movements`

- [x] 9.2 `domain:service` StockAdjustmentPolicy (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Valida quantidade >= 0; impede saída maior que estoque."
  - **Spec:** `stock-movements`

- [x] 9.3 `app:usecase` RecordStockEntry, RecordStockExit, PurchaseStock (~4h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Manual entry/exit RF-040/041; PurchaseStockUseCase RF-042 publica ProductPurchased."
  - **Spec:** `stock-movements`

- [x] 9.4 `infra:persistence` LegacyStockMovementMapper + IStockLegacyPort (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "entradas/saidas ACL; IStockLegacyPort AdjustStock, ReserveStock, ReleaseStock."
  - **Spec:** `stock-movements`, `catalog-open-host-service`

- [x] 9.5 `interface:controller` + Vue stock forms (~4h)
  - **Agent:** `Backend Controller (C#)` + `Frontend Form (Vue)`
  - **Prompt:** "/api/catalog/stock/movements, /purchase; forms entrada/saída/compra."
  - **Spec:** `stock-movements`, `catalog-web-ui`

## 10. Low stock (US-053)

- [x] 10.1 `domain:service` LowStockPolicy (~1h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "LowStockPolicy RN-021; emite LowStockDetected."
  - **Spec:** `low-stock-alerts`

- [x] 10.2 `app:query` ListLowStockProductsQuery (~1h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "Query produtos estoque < nivel_estoque RF-044."
  - **Spec:** `low-stock-alerts`

- [x] 10.3 `interface:page` Dashboard estoque baixo Vue (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "/catalog/inventory/low-stock dashboard PrimeVue."
  - **Spec:** `low-stock-alerts`, `catalog-web-ui`

## 11. OHS & parallel run (US-050)

- [x] 11.1 `app:usecase` ProductCatalogService FindByBarcode (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "IProductCatalogService OHS para Sales: FindByBarcode, GetAvailableStock."
  - **Spec:** `catalog-open-host-service`

- [x] 11.2 `test:e2e` Parallel run estoque read (~2h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "Compare stock quantities pilot tenant vs legacy; log mismatches."
  - **Spec:** `catalog-open-host-service`

## 12. Tests (US-050)

- [x] 12.1 `test:unit` Catalog domain + app (~3h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit: Product, ProfitMargin, StockAdjustment, Grades. Coverlet ≥95%."

- [x] 12.2 `test:e2e` Catalog CRUD + barcode (~2h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: CRUD product; duplicate barcode 409; by-barcode lookup."

## 13. Acceptance verification

- [x] 13.1 Validar US-050: CRUD produto/categoria, barcode único, preço aberto, foto, parallel read
- [x] 13.2 Validar US-051: grades Cor/Tamanho estoque por opção
- [x] 13.3 Validar US-052: movimentações + ProductPurchased event stub
- [x] 13.4 Validar US-053: dashboard estoque baixo RN-021
- [x] 13.5 Confirmar contrato OHS pronto para EP-006 PDV scan
