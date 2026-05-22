# Tasks — ep-007-finance (EP-007 Finance)

Referência: `docs/planning/loja-php/backlog.md` · US-070, US-071, US-072

## 1. Module setup

- [ ] 1.1 `infra:setup` Módulo Finance (~1h)
  - **Agent:** `Config New Module (C#)`
  - **Prompt:** "Crie RetailOps.Finance: Core, Application, Infrastructure; IReceivableRepository, IPayableRepository, ICommissionRepository."
  - **Spec:** `receivable-management`

## 2. Domain — shared VOs

- [ ] 2.1 `domain:vo` Finance VOs (~2h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "Money, DueDate, PaymentStatus Open|Settled, AccountType enum, Recurrence, AttachmentMeta."
  - **Spec:** `receivable-management`, `payable-management`

## 3. Domain — Receivable (US-070)

- [ ] 3.1 `domain:entity` Receivable aggregate (~3h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Receivable AR manual; AddAttachment; Settle(); child Attachment."
  - **Spec:** `receivable-management`

- [ ] 3.2 `domain:service` ReceivableSettlementService (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Valida baixa receber aberto; regras datas RF-070."
  - **Spec:** `receivable-management`

## 4. Domain — Payable (US-071)

- [ ] 4.1 `domain:entity` Payable aggregate (~3h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Payable AR Expense|Purchase|CommissionPayment; Recurrence; Attachment child."
  - **Spec:** `payable-management`

- [ ] 4.2 `domain:service` PayableSettlementService (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Baixa pagar; RN-061 vencimento futuro permanece Open na criação."
  - **Spec:** `payable-management`

## 5. Domain — Commission (US-072)

- [ ] 5.1 `domain:entity` Commission aggregate (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Commission AR linked SaleId, seller, amount, paid flag."
  - **Spec:** `commission-management`

- [ ] 5.2 `domain:service` CommissionSettlementService (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "PayCommission gera Payable tipo Pagamento RF-075."
  - **Spec:** `commission-management`

## 6. Application — Receivable (US-070)

- [ ] 6.1 `app:usecase` Receivable CRUD + SettleReceivable (~4h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Create, Update, Delete, SettleReceivable manual receivables."
  - **Spec:** `receivable-management`

- [ ] 6.2 `app:query` ListReceivablesQuery (~1h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ListReceivablesQuery filtros status, vencimento, paginação."
  - **Spec:** `receivable-management`

## 7. Application — Payable (US-071)

- [ ] 7.1 `app:usecase` Payable CRUD + SettlePayable (~4h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "CRUD despesas; SettlePayable RF-071."
  - **Spec:** `payable-management`

- [ ] 7.2 `app:query` ListPurchasesQuery (~1h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ListPurchasesQuery pagar.tipo Compra RF-072."
  - **Spec:** `payable-management`

## 8. Application — Commission & Cash flow (US-072)

- [ ] 8.1 `app:usecase` PayCommission, PayCommissionsBatch (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "PayCommission individual e lote RF-075."
  - **Spec:** `commission-management`

- [ ] 8.2 `app:query` ListCommissionsQuery, CashFlowQuery (~3h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ListCommissionsQuery; CashFlowAggregator RF-074 consolidado período."
  - **Spec:** `commission-management`, `cash-flow-consolidation`

## 9. Event handlers

- [ ] 9.1 `app:usecase` SaleCompletedHandler (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Handler SaleCompleted: sync Commission idempotente; coordena receivable projection."
  - **Spec:** `finance-event-handlers`, `sale-finalization`

- [ ] 9.2 `app:usecase` ProductPurchasedHandler (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Handler ProductPurchased: CreatePurchasePayable RN-061."
  - **Spec:** `finance-event-handlers`, `stock-movements`

- [ ] 9.3 `app:usecase` SaleCancelledFinancePort (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "IFinanceCancellationPort RemoveBySaleId RN-060; chamado por Sales CancelSale."
  - **Spec:** `finance-event-handlers`

## 10. Infrastructure — ACL (US-070)

- [ ] 10.1 `infra:persistence` LegacyReceivableMapper + Receivable aggregate (~8h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyReceivableMapper STI discriminator; ReceivableEfRepository; IFinanceLegacyPort."
  - **Spec:** `receivable-management`, `finance-legacy-acl`

- [ ] 10.2 `infra:persistence` LegacyPayableMapper + LegacyCommissionMapper (~6h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyPayableMapper, LegacyCommissionMapper pagar/comissoes ACL."
  - **Spec:** `payable-management`, `commission-management`, `finance-legacy-acl`

## 11. API

- [ ] 11.1 `interface:controller` FinanceReceivablesController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "CRUD /api/finance/receivables; POST settle; attachments sub-routes."
  - **Spec:** `receivable-management`

- [ ] 11.2 `interface:controller` FinancePayablesController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "CRUD /api/finance/payables; purchases list; settle."
  - **Spec:** `payable-management`

- [ ] 11.3 `interface:controller` FinanceCommissionsController + CashFlowController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "GET commissions; POST pay/batch; GET cash-flow."
  - **Spec:** `commission-management`, `cash-flow-consolidation`

## 12. Frontend Vue (US-070–072)

- [ ] 12.1 `interface:entity` + usecase + repository Finance Vue (~4h)
  - **Agent:** `Frontend Entity (Vue)` + `Frontend UseCase (Vue)` + `Frontend Repository (Vue)`
  - **Prompt:** "Entidades Receivable, Payable, Commission; repos /api/finance/*."

- [ ] 12.2 `interface:page` Contas a receber (~3h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "/finance/receivables DataTable + baixa + anexos RF-070."
  - **Spec:** `finance-web-ui`

- [ ] 12.3 `interface:page` + `interface:form-web` Despesas e compras (~4h)
  - **Agent:** `Frontend Page (Vue)` + `Frontend Form (Vue)`
  - **Prompt:** "/finance/payables e /finance/purchases RF-071/072."
  - **Spec:** `finance-web-ui`

- [ ] 12.4 `interface:page` Comissões + fluxo de caixa (~3h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "/finance/commissions pay batch; /finance/cash-flow RF-074/075."
  - **Spec:** `finance-web-ui`

## 13. Tests

- [ ] 13.1 `test:unit` Finance domain + settlement (~3h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit: ReceivableSettlement, PayableSettlement, CommissionSettlement, CashFlowAggregator. Coverlet ≥95%."

- [ ] 13.2 `test:e2e` Finance flows (~3h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: create receivable → settle; ProductPurchased handler → open payable; commission pay."

## 14. Acceptance verification

- [ ] 14.1 Validar US-070: CRUD receber manual, baixa, anexos RF-070
- [ ] 14.2 Validar US-071: despesas, compras, ProductPurchased RN-061
- [ ] 14.3 Validar US-072: comissões baixa/lote, fluxo caixa RF-074
- [ ] 14.4 Validar integração SaleCompleted/Cancelled com EP-006
- [ ] 14.5 Desbloquear EP-009 Reporting queries sobre Finance
