## Context

**RetailOps** — BC **Sales (PDV)** (Core). Maior épico do backlog; parallel run documentado em `migration-strategy.md` e `acl-design.md` (BC Sales — ACL crítica).

User stories: US-060 abrir caixa, US-061 carrinho, US-062 finalizar, US-063 sangria/fechamento, US-064 cancelar.

Invariantes (`ddd-tactical-model.md`):
- Sessão aberta obrigatória
- Carrinho vazio não finaliza
- Fiado exige CustomerId
- Troco ≥ 0
- Quebra: `contado − (abertura + vendido − sangrias)`

## Goals / Non-Goals

**Goals:**

- Paridade RF-050–RF-056, RF-073, RN-040–RN-046, RN-060
- ACL preserva fluxo legado: `itens_venda.venda=0` carrinho → finalize → `receber` tipo Venda
- Parallel run 2 semanas divergência ≤0,1%
- Vue PDV + Android PDV operacional
- Eventos de domínio para Finance e Catalog

**Non-Goals:**

- Trocas/devoluções (EP-008 Returns)
- Relatórios vendas (EP-009)
- Gateway cartão (stub legado)
- Impressão cupom fiscal

## Decisions

### 1. Módulo Sales

**Decisão:** `RetailOps.Sales.*`; referências application ports para Catalog, CRM, Identity — sem referência de domínio cruzada.

### 2. CashSession aggregate

**Decisão:** `CashSession` AR = sessão operador (`caixa` table):
- `Open(initialFloat, managerPinVerified)`
- `AddLine`, `RemoveLine`, `ConfirmGradeForLine`
- `RegisterWithdrawal`
- `Close(countedCash)` → `CashBreakage` via `CashSessionClosingPolicy`
- Cart lines = `SaleLine` com `venda=0` semantics

### 3. Sale aggregate

**Decisão:** Criado no `FinalizeSaleUseCase`; mapeia `receber` tipo Venda + itens confirmados; imutável após complete exceto cancel.

### 4. Cart & scan

**Decisão:**
- `AddItemToCartUseCase` chama `IProductCatalogService.FindByBarcode`
- Prefixo `2*{barcode}` → quantidade 2 (ScanQuantityPrefix VO)
- `ConfirmGradeForItemUseCase` para produtos com grade
- `CartStockReservationService` reserva via Catalog port

### 5. Finalize sale

**Decisão:** `FinalizeSaleUseCase` + `SaleFinalizationPolicy`:
- Valida payment method (Settings), customer se fiado, troco ≥ 0
- `CommissionCalculator` RN-046
- `LegacySaleAdapter.Finalize()` dual-write transacional
- Publica `SaleCompleted` com dados para Finance ACL

### 6. Parallel run

**Decisão:** `SaleParallelRunLogger` registra totais C# vs shadow PHP read; alert se diff >0,1%; feature flag roteia finalize para C# ou shadow compare mode.

### 7. API

- `POST /api/sales/cash-session/open`
- `GET/POST /api/sales/cash-session/current/cart`
- `POST /api/sales/cash-session/finalize`
- `POST /api/sales/cash-session/withdrawal`
- `POST /api/sales/cash-session/close`
- `GET /api/sales` (list), `POST /api/sales/{id}/cancel`

### 8. UI

**Decisão:** Vue rota `/pdv` fullscreen operador; Android feature `pdv` com camera barcode (ML Kit ou intent).

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| ACL `receber`+`itens_venda` inconsistente | Transação única LegacySaleAdapter; testes E2E vs DB legado |
| Parallel run divergência | Logger + 2 semanas pilot; rollback flag |
| Race estoque multi-terminal | Reserva Catalog + lock otimista |
| Comissão duplicada | Idempotency key finalize |

## Migration Plan

1. Cash session open/close → cart → finalize → cancel
2. Shadow mode parallel run
3. Pilot tenant 100% PDV C#
4. EP-011 cutover remove ACL dual-write

**Rollback:** flag PDV → PHP `pdv/buscar-codigo.php`.

## Open Questions

- Shadow compare mode vs full write? **Fase 1 shadow read; fase 2 dual-write logged**
- Offline Android PDV? **Fora escopo MVP — online only**
