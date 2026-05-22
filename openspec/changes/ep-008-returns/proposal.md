## Why

O legado registra **trocas 1:1** de produtos (`trocas/salvar.php`) com ajuste de estoque e movimentos de grade — operação Supporting pós-venda. O BC **Returns** encapsula RF-060/061 e RN-050/051 sem misturar lógica de troca no PDV (Sales) nem no catálogo, delegando estoque ao Catalog e cliente ao CRM.

## What Changes

- Novo módulo `RetailOps.Returns` (Core, Application, Infrastructure)
- Agregado `Exchange` com produto entrada/saída, grades opcionais, `CustomerId`
- VOs: `ExchangeId`, `ExchangeQuantity` (invariante = 1), `GradeSelection`
- Serviço `ExchangeStockPolicy` delegando `IStockLegacyPort` (+1 entrada, -1 saída)
- Caso de uso `RegisterExchangeUseCase` com auto-cadastro cliente via CRM port RN-051
- ACL: `LegacyExchangeMapper`, `LegacyExchangeAdapter`, `IReturnsLegacyPort`
- API: registrar, listar, excluir trocas
- Frontend Vue: formulário troca + listagem
- Evento `ProductExchanged` para auditoria
- Testes ≥95% + E2E fluxo troca

## Capabilities

### New Capabilities

- `product-exchange`: Registrar troca 1:1 com grades e ajuste estoque RF-060
- `exchange-management`: Listagem e exclusão de trocas RF-061
- `returns-legacy-acl`: Mapeamento `trocas` + `detalhes_grade` Troca Entrada/Saída
- `returns-web-ui`: Form e listagem Vue

### Modified Capabilities

- `customer-management`: RegisterExchange invoca FindOrCreateByCpf quando CPF informado RN-051
- `product-grades`: GradeMovementDetail registra Troca Entrada e Troca Saída via Catalog/Returns ACL RN-031

## Impact

- **Backend**: módulo Returns pequeno (P)
- **Catalog EP-005**: ajuste estoque ±1 e detalhes_grade
- **CRM EP-004**: FindOrCreateByCpf na troca
- **Dependências**: EP-000, EP-001, EP-004, EP-005 · **Release**: 2
