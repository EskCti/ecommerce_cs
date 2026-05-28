## Why

A US-051 (EP-005) previa grades com **até duas dimensões** e **estoque por combinação** (RF-033/034), mas a implementação atual guarda estoque em `GradeOption` de uma única dimensão. Isso atende casos simples (Cx, Unidade, só Cor), porém **falha para roupa** (Cor + Tamanho), onde o estoque é da **variante** Azul/P, Azul/M, etc. Sem corrigir isso, o PDV (EP-006) reserva estoque errado e o parallel run com o legado PHP diverge em `detalhes_grade`.

## What Changes

- Nova entidade de domínio **`GradeVariant`**: combinação de 1 ou 2 `GradeOption` com `StockQuantity`
- **BREAKING (API interna):** estoque deixa de ficar em `GradeOption` quando o produto tem 2 dimensões; passa para `GradeVariant`
- Retrocompatibilidade 1D: produto com uma dimensão continua com variante 1:1 por opção (Cx, Cor isolada)
- Sincronização automática de variantes ao adicionar/remover opções (produto cartesiano Cor × Tamanho)
- ACL legacy: mapear `itens_grade` + `itens_grade2` / `detalhes_grade` com duas FKs de opção
- OHS Catalog: reserva/liberação de estoque por **`variantId`**, não por `productId` genérico
- UI Vue: editor de **matriz** de estoque (dimensões + grid de combinações)
- Ajustes EP-006: `ConfirmGradeForItem` seleciona **variante** (2 passos ou picker combinado); Vue PDV + Android PDV
- Movimentações/compra: `GradeMovementDetail` referencia variante (duas opções quando 2D)

## Capabilities

### New Capabilities

_(nenhuma — extensão das capabilities de catálogo/PDV já existentes)_

### Modified Capabilities

- `product-grades`: Introduzir `GradeVariant`, estoque por combinação, máx. 2 dimensões, sync cartesiano
- `catalog-open-host-service`: Reserva/liberação e lookup de estoque por `variantId`; barcode response inclui variantes
- `catalog-web-ui`: Editor de matriz Cor × Tamanho; exibir variantes no grade editor
- `stock-movements`: Movimentações graduadas registram `GradeVariant` / par de opções em `detalhes_grade`
- `pdv-cart`: Confirmação de grade no carrinho por **variantId** (2 dimensões) com reserva na combinação correta

## Impact

- **Backend Catalog**: `Product`, `GradeOption`, `ConfigureProductGradeUseCase`, mappers legacy, schema dev (`itens_grade` 2D)
- **Backend Sales (EP-006)**: `ConfirmGradeForItemUseCase`, `CartStockReservationService`, `SaleLine.GradeOptionIds` → `GradeVariantId`
- **Frontend Vue**: `ProductsView` grade dialog, entidades/use cases catalog; `PdvView` fluxo 2D
- **Mobile Android**: `PdvScanScreen` grade dialog 2D
- **Dependências**: EP-005 (base), EP-006 (consumidor — aplicar patch nesta change antes de arquivar EP-006)
- **Release**: 1 (bloqueia PDV correto para produtos com grade 2D)
