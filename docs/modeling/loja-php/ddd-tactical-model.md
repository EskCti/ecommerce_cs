# Modelo Tático — Loja PHP (Sistema de Vendas SaaS)

**Baseado em**: `docs/discovery/ecommerce-legado-php/ddd-analysis.md` + modelo estratégico  
**Data da modelagem**: 2026-05-22

---

## BC Platform

**Subdomínio**: Gestão SaaS (Core plataforma)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: Platform                     │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • TenantId                                  │
│   • CompanyName                               │
│   • TaxDocument (CPF/CNPJ)                    │
│   • MonthlyFee (Money)                        │
│   • TrialPeriod (days)                        │
│   • BlockPolicy (days + message)              │
│   • ContractTemplate (HTML)                   │
│                                               │
│ Entities:                                     │
│   • Company [AR] — tenant lifecycle           │
│   • PlatformConfig [AR] — global settings     │
│   • Contract — child of Company               │
│                                               │
│ Aggregates:                                   │
│   • Company → Contract[]                      │
│   • PlatformConfig (singleton per scope)      │
│                                               │
│ Domain Services:                              │
│   • TrialExpirationPolicy                     │
│   • TenantBillingPolicy                       │
│   • TenantSuspensionPolicy                    │
│                                               │
│ Domain Events:                                │
│   • TrialRegistered                             │
│   • TenantInvoiceIssued                         │
│   • TenantSuspended                             │
│   • TenantReactivated                           │
│                                               │
│ Repository Ports:                             │
│   • ICompanyRepository                        │
│   • IPlatformConfigRepository                 │
│   • IContractRepository                       │
└──────────────────────────────────────────────┘
```

---

## BC Identity & Access

**Subdomínio**: Identidade e Acesso (Generic)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: Identity & Access            │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • UserId, TenantId                          │
│   • Email, Cpf, PersonName                    │
│   • PasswordHash (bcrypt — reimplementação)   │
│   • ManagerPin (separado de login)            │
│   • UserLevel (enum)                          │
│   • PermissionKey                             │
│   • ActiveStatus                              │
│                                               │
│ Entities:                                     │
│   • User [AR]                                 │
│   • PermissionGrant                           │
│   • Access (catálogo global)                  │
│   • AccessGroup                               │
│   • Role (cargo label)                        │
│                                               │
│ Aggregates:                                   │
│   • User → PermissionGrant[]                  │
│                                               │
│ Domain Services:                              │
│   • AuthorizationPolicy                       │
│   • ManagerAuthenticationService              │
│                                               │
│ Domain Events:                                │
│   • UserAuthenticated                           │
│   • UserDeactivated                             │
│   • PermissionsAssigned                         │
│                                               │
│ Repository Ports:                             │
│   • IUserRepository                           │
│   • IPermissionRepository                     │
└──────────────────────────────────────────────┘
```

---

## BC Store Settings

**Subdomínio**: Configuração da Loja (Supporting)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: Store Settings               │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • StoreName, StoreAddress, Cnpj             │
│   • DiscountType (% | fixed)                  │
│   • CommissionRate (0–100)                    │
│   • ReportFormat (PDF | HTML)                 │
│   • ApiToken                                  │
│   • ImagePath (logo)                          │
│                                               │
│ Entities:                                     │
│   • StoreConfig [AR]                          │
│   • PaymentMethod [AR] — nome + surcharge %   │
│   • CashRegisterTerminal [AR] — caixa físico   │
│                                               │
│ Aggregates:                                   │
│   • StoreConfig (1 per tenant)                │
│   • PaymentMethod                             │
│   • CashRegisterTerminal                      │
│                                               │
│ Domain Services:                              │
│   • PaymentSurchargeCalculator                │
│                                               │
│ Repository Ports:                             │
│   • IStoreConfigRepository                    │
│   • IPaymentMethodRepository                  │
│   • ICashRegisterTerminalRepository           │
└──────────────────────────────────────────────┘
```

---

## BC Catalog & Inventory

**Subdomínio**: Catálogo e Estoque (Core)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: Catalog & Inventory          │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • ProductId, Barcode (unique per tenant)    │
│   • ProductName, ProductDescription           │
│   • StockQuantity (>= 0)                      │
│   • SalePrice, CostPrice (Money)              │
│   • ProfitMargin (percentage)                 │
│   • StockAlertLevel                           │
│   • GradeDimensionName (Cor, Tamanho...)    │
│   • GradeOptionLabel                          │
│   • MovementReason                            │
│   • MovementType (Entry|Exit|Purchase|...)    │
│                                               │
│ Entities:                                     │
│   • Product [AR]                              │
│   • Category [AR]                             │
│   • GradeDimension                            │
│   • GradeOption (itens_grade)                 │
│   • StockMovement                             │
│   • GradeMovementDetail                       │
│                                               │
│ Aggregates:                                   │
│   • Product → GradeDimension[] → GradeOption[]│
│   • Category                                  │
│   • StockMovement (audit log per adjustment)  │
│                                               │
│ Domain Services:                              │
│   • StockAdjustmentPolicy                     │
│   • LowStockPolicy                            │
│   • ProfitMarginCalculator                    │
│   • ProductCatalogQuery (OHS para Sales)      │
│                                               │
│ Domain Events:                                │
│   • StockIncreased                              │
│   • StockDecreased                              │
│   • LowStockDetected                            │
│   • ProductPurchased                            │
│                                               │
│ Repository Ports:                             │
│   • IProductRepository                        │
│   • ICategoryRepository                       │
│   • IStockMovementRepository                  │
│                                               │
│ Factory:                                      │
│   • ProductFactory (com grades opcionais)     │
└──────────────────────────────────────────────┘
```

---

## BC Sales (PDV)

**Subdomínio**: Operação de PDV (Core)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: Sales (PDV)                  │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • CashSessionId, SaleId                     │
│   • Discount (% or Money)                     │
│   • ChangeAmount (>= 0)                       │
│   • CashBreakage                              │
│   • WarrantyDays                              │
│   • ScanQuantityPrefix (2*barcode)            │
│   • SaleLineTotal                             │
│                                               │
│ Entities:                                     │
│   • CashSession [AR]                          │
│   • SaleLine (cart item / sold line)          │
│   • Sale [AR] — header pós-finalização        │
│   • CashWithdrawal (sangria)                  │
│                                               │
│ Aggregates:                                   │
│   • CashSession → SaleLine[] (cart)           │
│                   → CashWithdrawal[]          │
│   • Sale → SaleLine[] (confirmed)             │
│           → PaymentTerms                      │
│                                               │
│ Domain Services:                              │
│   • SaleFinalizationPolicy                    │
│   • CartStockReservationService (calls Catalog)│
│   • CommissionCalculator                      │
│   • CashSessionClosingPolicy                  │
│                                               │
│ Domain Events:                                │
│   • CashSessionOpened                           │
│   • ItemAddedToCart                             │
│   • SaleCompleted                               │
│   • SaleCancelled                               │
│   • CashSessionClosed                           │
│   • CashWithdrawalRegistered                    │
│                                               │
│ Repository Ports:                             │
│   • ICashSessionRepository                    │
│   • ISaleRepository                           │
└──────────────────────────────────────────────┘
```

**Invariantes do aggregate Sale / CashSession**

- Sessão aberta obrigatória para vender
- Carrinho vazio não finaliza
- Fiado exige CustomerId
- Troco ≥ 0
- Quebra calculada no fechamento: `contado − (abertura + vendido − sangrias)`

---

## BC Returns

**Subdomínio**: Trocas e Devoluções (Supporting)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: Returns                      │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • ExchangeId                                │
│   • ExchangeQuantity (= 1, invariante)        │
│   • GradeSelection (optional 2 dimensions)    │
│                                               │
│ Entities:                                     │
│   • Exchange [AR]                             │
│                                               │
│ Aggregates:                                   │
│   • Exchange (in + out product refs)          │
│                                               │
│ Domain Services:                              │
│   • ExchangeStockPolicy (delegates Catalog)   │
│                                               │
│ Domain Events:                                │
│   • ProductExchanged                            │
│                                               │
│ Repository Ports:                             │
│   • IExchangeRepository                       │
└──────────────────────────────────────────────┘
```

---

## BC Finance

**Subdomínio**: Financeiro da Loja (Supporting)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: Finance                      │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • Money, DueDate                            │
│   • PaymentStatus (Open|Settled)              │
│   • AccountType (Sale|Purchase|Expense|       │
│     CommissionPayment|TenantInvoice)          │
│   • Recurrence (days)                         │
│   • AttachmentMeta (path, expiry)             │
│                                               │
│ Entities:                                     │
│   • Receivable [AR]                           │
│   • Payable [AR]                              │
│   • Commission [AR]                           │
│   • Attachment                                │
│   • Frequency (reference)                     │
│                                               │
│ Aggregates:                                   │
│   • Receivable → Attachment[]                 │
│   • Payable → Attachment[]                    │
│   • Commission                                │
│                                               │
│ Domain Services:                              │
│   • ReceivableSettlementService               │
│   • PayableSettlementService                  │
│   • CommissionSettlementService               │
│   • CashFlowAggregator                        │
│                                               │
│ Domain Events:                                │
│   • ReceivableCreated                           │
│   • ReceivableSettled                           │
│   • PayableCreated                              │
│   • PayableSettled                              │
│   • CommissionAccrued                           │
│   • CommissionPaid                              │
│                                               │
│ Repository Ports:                             │
│   • IReceivableRepository                     │
│   • IPayableRepository                        │
│   • ICommissionRepository                     │
│                                               │
│ ACL:                                          │
│   • SaleSettlementAdapter (consome SaleCompleted)│
└──────────────────────────────────────────────┘
```

---

## BC CRM

**Subdomínio**: Cadastro de Partes (Supporting)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: CRM                          │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • CustomerId, SupplierId                    │
│   • PersonName, Cpf, Email, Phone, Address    │
│   • PersonType (Individual|Company)           │
│                                               │
│ Entities:                                     │
│   • Customer [AR]                             │
│   • Supplier [AR]                             │
│                                               │
│ Aggregates:                                   │
│   • Customer → Attachment[] (tipo Cliente)    │
│   • Supplier                                  │
│                                               │
│ Domain Services:                              │
│   • CustomerRegistrationPolicy                │
│                                               │
│ Domain Events:                                │
│   • CustomerRegistered                          │
│   • CustomerFoundByCpf                          │
│                                               │
│ Repository Ports:                             │
│   • ICustomerRepository                       │
│   • ISupplierRepository                       │
└──────────────────────────────────────────────┘
```

---

## BC Reporting

**Subdomínio**: Relatórios e BI (Generic)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: Reporting (Read Side)        │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • DateRange, ReportFilter                   │
│   • ReportFormat                              │
│                                               │
│ Read Models / Projections:                    │
│   • SalesReportRow                            │
│   • ReceivablesReportRow                      │
│   • PayablesReportRow                         │
│   • ProfitStatement                           │
│   • LowStockRow                               │
│   • CashSessionReportRow                      │
│   • CommissionReportRow                       │
│   • ExchangeReportRow                         │
│   • CustomerListRow                           │
│   • ReceiptDocument                           │
│                                               │
│ Queries (CQRS):                               │
│   • ReportSalesQuery                          │
│   • ReportReceivablesQuery                    │
│   • ReportPayablesQuery                       │
│   • ReportProfitQuery                         │
│   • ReportLowStockQuery                       │
│   • ReportCashSessionsQuery                   │
│   • ReportCommissionsQuery                    │
│   • ReportExchangesQuery                      │
│   • GenerateReceiptQuery                      │
│                                               │
│ Infrastructure:                               │
│   • PdfRendererPort (dompdf → adapter)        │
└──────────────────────────────────────────────┘
```

---

## BC Notifications

**Subdomínio**: Notificações (Generic)

```
┌──────────────────────────────────────────────┐
│ Bounded Context: Notifications                │
├──────────────────────────────────────────────┤
│ Value Objects:                                │
│   • PhoneNumber (E.164 BR)                    │
│   • MessageTemplate                           │
│   • ScheduledDateTime                         │
│   • DigestContent                             │
│                                               │
│ Entities:                                     │
│   • NotificationSchedule [AR]                 │
│   • DailyDigestLog                            │
│                                               │
│ Domain Services:                              │
│   • DailyDigestComposer (ACL queries)         │
│                                               │
│ Domain Events:                                │
│   • DigestSent                                  │
│   • MessageScheduled                            │
│                                               │
│ Repository Ports:                             │
│   • INotificationLogRepository                │
│                                               │
│ Infrastructure (ACL):                           │
│   • WhatsAppGatewayPort → api.enviame.com.br  │
└──────────────────────────────────────────────┘
```

---

## Matriz de integração tática (eventos cross-BC)

| Evento | Produtor | Consumidor | Ação |
| ------ | -------- | ---------- | ---- |
| `SaleCompleted` | Sales | Finance | Criar Receivable + Commission |
| `SaleCancelled` | Sales | Finance + Catalog | Estornar Receivable/Commission; devolver estoque |
| `SaleCompleted` | Sales | Catalog | Confirmar baixa estoque (se reserva diferida) |
| `ProductPurchased` | Catalog | Finance | Criar Payable(Compra) |
| `ProductExchanged` | Returns | Catalog | AdjustStock ±1 |
| `TenantInvoiceIssued` | Platform | Finance | Receivable(TenantInvoice) |
| `TenantSuspended` | Platform | Identity | Desativar users do tenant |
| `LowStockDetected` | Catalog | Notifications | Incluir no digest |
| `ReceivableDueToday` | Finance | Notifications | Incluir no digest |
