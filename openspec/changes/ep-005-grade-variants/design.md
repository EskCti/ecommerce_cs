## Context

**RetailOps** — complemento da US-051 no BC **Catalog**. O legado PHP (`cat_grade`, `itens_grade`, `cat_grade2`, `itens_grade2`, `detalhes_grade`) trata **duas dimensões** e movimentações com **par de opções**. A implementação C# atual modela N dimensões mas persiste **estoque em cada `GradeOption`**, o que não representa combinações.

**Stakeholders:** gestor (cadastro matriz), operador PDV (escolha Cor + Tamanho), estoque (reserva/compra/cancelamento).

## Goals / Non-Goals

**Goals:**

- Paridade RF-033/034/RN-031 com estoque por **combinação**
- Suportar **1 dimensão** (Cx, Unidade, só Cor) e **2 dimensões** (Cor + Tamanho)
- Retrocompat: migrar produtos 1D existentes para variantes 1:1 sem perda de estoque
- PDV reserva/commit/release na variante correta
- ACL `detalhes_grade` com `itens_grade` + `itens_grade2`

**Non-Goals:**

- 3+ dimensões (Material, Numeração extra além de Cor+Tamanho)
- SKU/ERP externo, código por variante
- Impressão de etiqueta por variante
- Reescrever EP-006 inteiro — apenas patch de grade no carrinho

## Decisions

### 1. GradeVariant como entidade filha do Product

**Decisão:** `GradeVariant` entity com:
- `ProductId`
- `IReadOnlyList<Guid> OptionIds` (ordenados: dimensão 1, dimensão 2 opcional)
- `StockQuantity`
- `SkuLabel` derivado (opcional, ex. "Azul / P") — não persistir como VO separado no MVP

**Alternativa rejeitada:** estoque só em `GradeOption` com “opção composta” string "Azul-P" — viola modelo legado e UX.

### 2. Estoque: Option vs Variant

| Dimensões | Onde fica estoque |
|-----------|-------------------|
| 0 | `Product.Stock` |
| 1 | `GradeVariant` 1:1 por opção (auto-criada) |
| 2 | **Somente** `GradeVariant`; `GradeOption.Stock` ignorado (sempre 0 ou não exposto) |

### 3. Sincronização cartesiana

**Decisão:** `GradeVariantSyncService` ao `AddOption` / `RemoveOption` / `RemoveDimension`:
- 1D: 1 variante por opção
- 2D: produto cartesiano dim1 × dim2; novas células nascem com estoque 0
- Remover opção remove variantes que a referenciam

### 4. API grades

Estender `ConfigureProductGradeInputDto`:
- `AddVariant`, `AdjustVariantStock`, `RemoveVariant`
- `AddOption` deixa de aceitar `stock` quando produto tem 2 dimensões (estoque só na matriz)

Response `ProductOutputDto.GradeVariants[]` com `{ id, optionIds[], label, stock }`.

### 5. OHS Catalog (Sales)

**Decisão:** `IProductCatalogService`:
- `ReserveStockAsync(tenantId, variantId, quantity)`
- `ReleaseStockAsync(tenantId, variantId, quantity)`
- `GetVariantStockAsync(tenantId, variantId)`
- `FindVariantsByProductAsync(productId)` para PDV picker

Manter overload por `productId` apenas para produtos **sem** grade.

### 6. PDV (EP-006 patch)

**Decisão:** `ConfirmGradeForItemInputDto` passa a aceitar `GradeVariantId` **ou** `GradeOptionIds[]` (2 ids) resolvido server-side para variante.

UI: wizard 2 passos (Cor → Tamanho) ou matriz compacta no modal.

### 7. ACL legacy

**Decisão:**
- Tabelas dev: reutilizar `cat_grade` para até 2 linhas por produto (dimensões); `itens_grade` por dimensão
- `LegacyGradeVariantMapper`: combinação → par `(itens_grade_id, itens_grade_id_2)`; estoque na variante mapeado conforme legado (movimentações via `detalhes_grade`)
- Migração dados 1D existentes: criar variantes espelhando `GradeOption.Stock` atual

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Breaking API grade editor | Versionar DTO; UI envia estoque só via matriz quando 2D |
| Cartesiano grande (10×10) | Limitar opções por dimensão (ex. máx. 20 cada) |
| EP-006 já implementado com optionIds | Patch coordenado nesta change; testes E2E PDV grade 2D |
| Divergência ACL | Testes integração `detalhes_grade` com 2 FKs |

## Migration Plan

1. Deploy schema + entidade `GradeVariant`
2. Script/migration: copiar `GradeOption.Stock` → `GradeVariant` para produtos 1D
3. Zerar estoque em opções quando 2ª dimensão é adicionada; gerar matriz
4. Atualizar Vue editor + PDV
5. Validar parallel run compra/venda grade 2D

**Rollback:** feature flag `UseGradeVariants=false` cai para modelo 1D (somente produtos single-dimension).

## Open Questions

- Persistir estoque 2D no legado em tabela dedicada ou só via soma de movimentações? **Fase 1: variantes C# + detalhes_grade; estoque derivado de movimentações como PHP**
- Limite máximo de opções por dimensão? **Default 30**
