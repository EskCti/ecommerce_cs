# Tasks — ep-005-grade-variants (US-051 complemento — GradeVariant)

Referência: `docs/planning/loja-php/backlog.md` · US-051 · RF-033, RF-034, RN-031 · patch EP-006 PDV

## 1. Domain — GradeVariant (Catalog)

- [ ] 1.1 `domain:entity` GradeVariant (~3h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Criar GradeVariant em Product aggregate: OptionIds ordenados (1 ou 2), StockQuantity, label derivado; regra máx. 2 dimensões."
  - **Spec:** `product-grades`

- [ ] 1.2 `domain:service` GradeVariantSyncService (~3h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Sincronizar variantes cartesianas ao AddOption/RemoveOption/RemoveDimension; 1D = 1:1 opção-variante."
  - **Spec:** `product-grades`

- [ ] 1.3 `domain:entity` Ajustar Product + GradeOption (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Product expõe GradeVariants; GradeOption sem estoque quando 2D; AddGradeVariant, AdjustVariantStock, RemoveVariant."
  - **Spec:** `product-grades`

## 2. Application — Configure grades & OHS

- [ ] 2.1 `app:usecase` Estender ConfigureProductGradeUseCase (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Actions AddVariant, AdjustVariantStock, RemoveVariant; AddOption rejeita stock quando 2 dimensões."
  - **Spec:** `product-grades`

- [ ] 2.2 `app:usecase` ResolveGradeVariantUseCase (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Resolve variante por GradeVariantId ou lista ordenada de OptionIds; retorna stock disponível."
  - **Spec:** `catalog-open-host-service`

- [ ] 2.3 `app:usecase` IProductCatalogService por variantId (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "ReserveStockAsync/ReleaseStockAsync/GetVariantStockAsync por variantId; manter productId para não-graduado."
  - **Spec:** `catalog-open-host-service`

- [ ] 2.4 `core:dto` GradeVariant DTOs (~1h)
  - **Agent:** `Core DTO (C#)`
  - **Prompt:** "GradeVariantOutputDto, AdjustVariantStockInputDto; ProductOutputDto inclui gradeVariants[]."
  - **Spec:** `product-grades`

## 3. Infrastructure — ACL legacy 2D

- [ ] 3.1 `infra:persistence` Schema + LegacyGradeVariantMapper (~4h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyGradeVariantRow ou mapeamento itens_grade+itens_grade2; detalhes_grade com duas FKs; legacy-dev-schema.sql."
  - **Spec:** `product-grades`, `stock-movements`

- [ ] 3.2 `infra:persistence` Migração estoque 1D → Variant (~2h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "Bootstrap/migration: copiar GradeOption.Stock existente para GradeVariant 1:1 em produtos single-dimension."
  - **Spec:** `product-grades`

- [ ] 3.3 `infra:persistence` StockLegacyAdapter variant stock (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "Implementar Reserve/Release/Adjust real por GradeVariant; GradeMovementDetail com par de opções."
  - **Spec:** `catalog-open-host-service`, `stock-movements`

## 4. API

- [ ] 4.1 `interface:controller` Estender CatalogGradesController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "Endpoints ou actions para matriz de variantes; response inclui gradeVariants."
  - **Spec:** `product-grades`

## 5. Sales patch (EP-006)

- [ ] 5.1 `app:usecase` ConfirmGradeForItemUseCase variantId (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Confirmar por GradeVariantId ou 2 OptionIds; reservar via IProductCatalogService variant; SaleLine guarda VariantId."
  - **Spec:** `pdv-cart`

- [ ] 5.2 `domain:entity` SaleLine GradeVariantId (~1h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "SaleLine referencia GradeVariantId quando graduado; CartStockReservationService usa variant."
  - **Spec:** `pdv-cart`

## 6. Frontend Vue — Grade matrix (Catalog)

- [ ] 6.1 `interface:entity` GradeVariant Vue (~1h)
  - **Agent:** `Frontend Entity (Vue)`
  - **Prompt:** "Entidade GradeVariantEntity + tipos matriz; fromApi com optionIds, label, stock."

- [ ] 6.2 `interface:usecase` + `interface:repository` Grade variants (~2h)
  - **Agent:** `Frontend UseCase (Vue)` + `Frontend Repository (Vue)`
  - **Prompt:** "ConfigureVariantStock, ListVariants; estender grade-http.repository e product output parsing."

- [ ] 6.3 `interface:form-web` Matriz Cor × Tamanho (~4h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "ProductsView grade dialog: dimensões + grid editável estoque por célula; 1D mantém fluxo atual."
  - **Spec:** `catalog-web-ui`

## 7. Frontend Vue + Mobile — PDV grade 2D

- [ ] 7.1 `interface:form-web` PdvView picker 2 dimensões (~3h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "PdvView: wizard Cor→Tamanho ou matriz compacta; confirm-grade envia variantId ou optionIds."
  - **Spec:** `pdv-cart`

- [ ] 7.2 `interface:mobile-form` Android grade 2D (~3h)
  - **Agent:** `Mobile Form (Android)`
  - **Prompt:** "PdvScanScreen: dialog sequencial dimensão 1 e 2; confirm API com variant; StateFlow."
  - **Spec:** `pdv-cart`

## 8. Tests

- [ ] 8.1 `test:unit` GradeVariantSync + policies (~3h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit: cartesiano 2×2, migração 1D, limite 2 dimensões, ResolveVariant. Coverlet ≥95% domain."

- [ ] 8.2 `test:e2e` Grade matrix API + PDV 2D (~4h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: produto Cor×Tamanho, ajustar estoque matriz, PDV add+confirm variant, finalize; fiado inalterado."

- [ ] 8.3 `test:unit-web` Grade matrix entity (~1h)
  - **Agent:** `Unit Tests (TypeScript)`
  - **Prompt:** "Vitest: GradeVariantEntity.fromApi, label combinação, validação célula matriz."

## 9. Acceptance verification

- [ ] 9.1 Validar US-051: produto Cx (1D) mantém estoque por variante 1:1
- [ ] 9.2 Validar US-051: produto roupa Cor×Tamanho com estoque independente por célula
- [ ] 9.3 Validar PDV: reserva/liberação na combinação correta (não na dimensão isolada)
- [ ] 9.4 Validar movimentação/compra: detalhes_grade com par de opções
- [ ] 9.5 Validar retrocompat: produtos cadastrados antes da change continuam vendáveis
