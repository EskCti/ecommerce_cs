## Why

O **BC Sales (PDV)** é o coração do MVP Release 1: abertura de caixa, scan de produtos, finalização com pagamento/fiado/comissão, sangria, fechamento e cancelamento. É o épico **GG** com **parallel run obrigatório** contra o PHP — divergência >0,1% em 2 semanas bloqueia cutover. Depende de Settings (caixas) e Catalog (produtos/estoque); desbloqueia Finance via `SaleCompleted`.

## What Changes

- Novo módulo `RetailOps.Sales` (Core, Application, Infrastructure)
- Agregados `CashSession` (carrinho + sangrias) e `Sale` (pós-finalização)
- Entidades `SaleLine`, `CashWithdrawal`; VOs de desconto, troco, quebra, comissão
- Serviços: `SaleFinalizationPolicy`, `CommissionCalculator`, `CartStockReservationService`, `CashSessionClosingPolicy`
- ACL crítica: `LegacySaleAdapter`, mappers `caixa`/`itens_venda`/`receber`/`sangrias`/`comissoes`
- **`SaleParallelRunLogger`** — compara totais shadow PHP vs C#
- API PDV + integração Catalog OHS, Identity (PIN gerente), CRM (cliente fiado)
- Frontend Vue PDV completo; Android PDV (abertura, scan, checkout)
- Eventos: `SaleCompleted` → Finance stub; `SaleCancelled` → estoque Catalog
- Testes ≥95%, E2E fluxo PDV, parallel run 2 semanas

## Capabilities

### New Capabilities

- `cash-session-management`: Abrir/fechar sessão, fundo inicial, sangria, cálculo quebra RN-040
- `pdv-cart`: Scan barcode, prefixo quantidade `2*`, confirmação grade, reserva estoque
- `sale-finalization`: Finalizar venda, fiado, troco, comissão, ACL dual-write
- `sale-cancellation`: Cancelar venda, devolver estoque RN-060
- `sales-legacy-acl`: LegacySaleAdapter, ISalesLegacyPort, parallel run logger
- `pdv-web-ui`: Interface Vue operador/gerente PDV
- `pdv-mobile`: Android abertura caixa, scan, finalização

### Modified Capabilities

- `catalog-open-host-service`: Reserva/liberação estoque acoplada ao ciclo de vida do carrinho CashSession
- `user-authentication`: Verificação PIN gerente obrigatória em abertura/fechamento de caixa (integração EP-001)

## Impact

- **Backend**: módulo Sales + ACL mais crítica do projeto
- **Catalog EP-005**: `IStockLegacyPort` chamado em add item / finalize / cancel
- **CRM EP-004**: `CustomerId` obrigatório em fiado
- **Finance EP-007**: consome `SaleCompleted` (interface neste épico)
- **Dependências**: EP-000, EP-001, EP-003, EP-005 (EP-004 recomendado para fiado)
- **Release**: 1 — critério go-live MVP
