## Why

O legado concentra financeiro em tabelas polimórficas `receber`, `pagar` e `comissoes` — difícil de modelar, mas crítico para operação pós-PDV. O **BC Finance** (Supporting) absorve contas a receber/pagar, baixas, comissões e fluxo de caixa (RF-070–RF-075), consumindo eventos de **Sales** (`SaleCompleted`) e **Catalog** (`ProductPurchased`) sem replicar o modelo polimórfico no domínio.

## What Changes

- Novo módulo `RetailOps.Finance` (Core, Application, Infrastructure)
- Agregados tipados: `Receivable` (manual + sale-linked), `Payable` (expense/purchase/commission payment), `Commission`
- VOs: `Money`, `DueDate`, `PaymentStatus`, `AccountType`, `Recurrence`, `AttachmentMeta`
- Serviços: `ReceivableSettlementService`, `PayableSettlementService`, `CommissionSettlementService`, `CashFlowAggregator`
- ACL discriminators: `LegacyReceivableMapper`, `LegacyPayableMapper`, `LegacyCommissionMapper`, `IFinanceLegacyPort`
- Handlers: `SaleCompleted` → SaleReceivable; `ProductPurchased` → PurchasePayable
- API: CRUD receber/pagar, baixa, comissões, fluxo de caixa
- Frontend Vue: contas a receber, despesas, compras, comissões, fluxo
- Testes ≥95% + E2E baixa e fluxo

## Capabilities

### New Capabilities

- `receivable-management`: CRUD contas a receber manuais, baixa, anexos RF-070
- `payable-management`: CRUD despesas/compras, recorrência, anexos RF-071/072
- `commission-management`: Comissões vendedor, baixa individual/lote RF-075
- `cash-flow-consolidation`: Fluxo de caixa consolidado RF-074
- `finance-legacy-acl`: Mapeamento STI legado sem vazar `receber`/`pagar` ao domínio
- `finance-web-ui`: UI Vue financeiro tenant
- `finance-event-handlers`: Integração SaleCompleted e ProductPurchased

### Modified Capabilities

- `sale-finalization`: Finance persiste SaleReceivable e Commission via handler pós-SaleCompleted (complementa ACL Sales)
- `stock-movements`: ProductPurchased cria PurchasePayable no Finance (completa stub EP-005)

## Impact

- **Backend**: módulo Finance + ACL mais complexa após Sales
- **Sales EP-006**: emite `SaleCompleted`; cancel remove via RN-060 (coordenação cross-BC)
- **Catalog EP-005**: emite `ProductPurchased`
- **Platform EP-002**: TenantInvoice permanece Platform — Finance ACL roteia tipo Empresa
- **Reporting EP-009**: depende de queries Finance/CQRS
- **Dependências**: EP-000, EP-001, EP-006 (Release 2)
