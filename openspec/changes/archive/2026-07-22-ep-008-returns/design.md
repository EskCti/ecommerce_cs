## Context

**RetailOps** — BC **Returns** (Supporting, Trocas e Devoluções). Release 2; depende de Catalog (produtos/grades/estoque) e CRM (cliente). Uma user story principal no backlog (US-080) + listagem/exclusão RF-061.

Legado: `trocas`, `detalhes_grade` tipos Troca Entrada/Saída.

Referências: `ddd-tactical-model.md` (BC Returns), `acl-design.md` (BC Returns).

## Goals / Non-Goals

**Goals:**

- Paridade RF-060, RF-061, RN-050, RN-051
- Troca sempre quantidade 1 entrada + 1 saída
- Grades opcionais em ambos os lados
- Auto-create customer by CPF via CRM port
- List/delete exchanges
- Vue form + list

**Non-Goals:**

- Devolução com reembolso financeiro (sem fluxo legado equivalente completo)
- Troca N:N ou quantidades variáveis
- Mobile Android neste épico
- Integração Finance (troca não gera receber/pagar no legado)

## Decisions

### 1. Módulo Returns

**Decisão:** `RetailOps.Returns.*`; tenant-scoped.

### 2. Exchange aggregate

**Decisão:** `Exchange` AR:
- `CustomerId` (required after CRM resolution)
- `ProductInId`, `ProductOutId`
- optional `GradeSelectionIn`, `GradeSelectionOut`
- invariant `ExchangeQuantity = 1` each side RN-050
- `Register()` validates stock out available

### 3. ExchangeStockPolicy

**Decisão:** Application/domain service calls:
- `IStockLegacyPort.Increase(productIn, gradeIn, 1)`
- `IStockLegacyPort.Decrease(productOut, gradeOut, 1)`
- Records `GradeMovementDetail` Troca Entrada/Saída via Catalog ACL

### 4. Customer auto-create RN-051

**Decisão:** `RegisterExchangeUseCase` calls `ICustomerProvisioningPort.FindOrCreateByCpf` from CRM application layer when CPF provided without existing customer.

### 5. API

- `POST /api/returns/exchanges`
- `GET /api/returns/exchanges`
- `DELETE /api/returns/exchanges/{id}`

### 6. Delete exchange

**Decisão:** Delete reverses stock adjustment (+1 out, -1 in) if business allows — match legacy exclusão behavior [inferido: reverse stock].

### 7. UI

**Decisão:** Vue `/returns/exchanges` list + modal form with product pickers, grade selectors, customer CPF lookup.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Stock out insufficient | Validate before persist |
| Delete reverses stock incorrectly | Transaction + integration test |
| CRM port unavailable | Require existing CustomerId fallback |

## Migration Plan

1. Domain → ACL → API → Vue → tests
2. Pilot flag Returns BC

**Rollback:** flag off; PHP `trocas/` ativo.

## Open Questions

- Delete reverses stock? **Sim, alinhado exclusão legado [inferido]**
- Permission keys? **Mapear menu trocas legado**
