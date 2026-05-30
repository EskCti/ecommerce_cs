## Context

**RetailOps** — BC **Finance** (Supporting). Release 2; depende de EP-006 Sales operacional. Legado: módulo financeiro com `receber`/`pagar` polimórficos — `acl-design.md` define discriminators e anti-corruption.

**Princípio:** Domínio Finance usa agregados tipados (`ManualReceivable`, `PurchasePayable`, etc.); ACL traduz para/do legado. **Não** expor `receber.tipo` no Core.

Referências: `ddd-tactical-model.md` (BC Finance), `acl-design.md` (BC Finance section).

## Goals / Non-Goals

**Goals:**

- Paridade RF-070, RF-071, RF-072, RF-074, RF-075, RN-061
- Baixa de receber/pagar/comissões
- Anexos metadata (path) em receber/pagar
- Handler `SaleCompleted` → receivable + commission records
- Handler `ProductPurchased` → purchase payable RN-061
- `CashFlowAggregator` query RF-074
- UI Vue módulo financeiro

**Non-Goals:**

- Relatórios PDF completos (EP-009 Reporting)
- WhatsApp digest (EP-010)
- Tenant billing SAS tipo Empresa (EP-002 Platform — ACL route only)
- Gateway pagamento online

## Decisions

### 1. Módulo Finance

**Decisão:** `RetailOps.Finance.*`; tenant-scoped; application handlers para eventos cross-BC.

### 2. Receivable aggregate

**Decisão:** `Receivable` AR para contas manuais; `SaleReceivable` factory/handler cria a partir de `SaleCompleted` — referência `SaleId`, não duplicar lógica PDV.

**Settlement:** `SettleReceivableUseCase` → `ReceivableSettlementService` → ACL baixa `receber`.

**Tipos legado Venda/Empresa:** Venda via Sales handler; Empresa roteada para Platform port; manual via Finance CRUD.

### 3. Payable aggregate

**Decisão:** `Payable` AR com `AccountType`: Expense, Purchase, CommissionPayment.

**Purchase:** `ProductPurchasedHandler` cria PurchasePayable; vencimento futuro → `PaymentStatus.Open` RN-061.

**Recurrence:** `Recurrence` VO de `frequencias.dias` para despesas recorrentes.

### 4. Commission aggregate

**Decisão:** `Commission` AR linked to `SaleId`; `PayCommissionUseCase` gera Payable tipo Pagamento e marca commission paid RF-075.

### 5. Cash flow

**Decisão:** `CashFlowAggregator` CQRS read model consolida settled movements por período — não muta agregados.

### 6. ACL

**Decisão:** `IFinanceLegacyPort` + mappers STI infrastructure-only:

```csharp
LegacyReceivableType { Venda, Empresa, Other }
// Venda → coordination with Sales (already written) or read-only sync
// Empresa → IPlatformInvoicePort
// Other → ManualReceivable
```

### 7. API routes

- `/api/finance/receivables` CRUD + settle
- `/api/finance/payables` CRUD + settle
- `/api/finance/purchases` list (tipo Compra)
- `/api/finance/commissions` list + pay
- `/api/finance/cash-flow?from&to`

### 8. Attachments

**Decisão:** `Attachment` child entity metadata; upload file deferred (path string MVP).

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Duplicação Sale receber Sales vs Finance | Sales ACL writes; Finance reads/projections; handler idempotent |
| Polimorfismo legado | Strict ACL; domain never sees tipo string |
| Cross-BC cancel RN-060 | Sales CancelSale orchestrates Finance delete port |
| Commission double pay | Idempotency on PayCommission |

## Migration Plan

1. Event handlers (SaleCompleted, ProductPurchased)
2. Manual CRUD + settlement
3. Commissions + cash flow
4. UI Vue
5. Pilot tenant Finance BC flag

**Rollback:** flag off; PHP `receber/`/`pagar/` ativos.

## Open Questions

- Sale receivable created in Sales ACL or Finance handler? **Sales ACL write on finalize; Finance domain projection sync for queries**
- Batch commission pay scope? **MVP single + multi-select batch**
