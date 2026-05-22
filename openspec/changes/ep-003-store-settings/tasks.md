# Tasks — ep-003-store-settings (EP-003 Store Settings)

Referência: `docs/planning/loja-php/backlog.md` · US-030, US-031

## 1. Module setup

- [ ] 1.1 `infra:setup` Módulo StoreSettings (~1h)
  - **Agent:** `Config New Module (C#)`
  - **Prompt:** "Crie RetailOps.StoreSettings: Core, Application, Infrastructure; tenant-scoped repos; referência Shared.Kernel."
  - **Spec:** `store-config`

## 2. Domain — StoreConfig (US-030)

- [ ] 2.1 `domain:vo` Store VOs (~2h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "StoreName, Cnpj, DiscountType enum, CommissionRate, ReportFormat, ApiToken, ImagePath com Create() Result<T>."
  - **Spec:** `store-config`

- [ ] 2.2 `domain:entity` StoreConfig aggregate (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "StoreConfig AR 1/tenant: UpdateGeneralInfo, UpdateDiscountSettings, UpdateReportSettings, UpdateIntegrationToken."
  - **Spec:** `store-config`

## 3. Domain — PaymentMethod (US-031)

- [ ] 3.1 `domain:entity` PaymentMethod aggregate (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "PaymentMethod AR: Name, SurchargePercent; validar nome único; Rename(), UpdateSurcharge()."
  - **Spec:** `payment-methods`

- [ ] 3.2 `domain:service` PaymentSurchargeCalculator (~1h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "PaymentSurchargeCalculator RN-047: Calculate(Money base, PaymentMethod) → Money total."
  - **Spec:** `payment-methods`

## 4. Domain — CashRegisterTerminal (US-031)

- [ ] 4.1 `domain:entity` CashRegisterTerminal aggregate (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "CashRegisterTerminal AR: Name, TerminalStatus Open/Closed, AssignedOperatorId optional UserId."
  - **Spec:** `cash-register-terminals`

## 5. Application layer

- [ ] 5.1 `app:usecase` StoreConfig use cases (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "GetStoreConfigUseCase, UpdateStoreConfigUseCase; create-on-first-read."
  - **Spec:** `store-config`

- [ ] 5.2 `app:usecase` PaymentMethod use cases (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Create, Update, Delete, List PaymentMethod use cases tenant-scoped."
  - **Spec:** `payment-methods`

- [ ] 5.3 `app:usecase` CashRegisterTerminal use cases (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "CRUD CashRegisterTerminal; valida operador via IUserLookupPort Identity."
  - **Spec:** `cash-register-terminals`

- [ ] 5.4 `app:query` List queries (~1h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ListPaymentMethodsQuery, ListCashRegisterTerminalsQuery com projeções DTO."
  - **Spec:** `payment-methods`, `cash-register-terminals`

## 6. Infrastructure

- [ ] 6.1 `infra:persistence` Legacy ACL Store Settings (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyStoreConfigMapper, LegacyPaymentMethodMapper, LegacyCashRegisterTerminalMapper; IStoreSettingsLegacyPort."
  - **Spec:** `store-config`, `payment-methods`, `cash-register-terminals`

## 7. API

- [ ] 7.1 `interface:controller` StoreSettingsController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "GET/PUT /api/settings/store-config; CRUD payment-methods e cash-registers; Authorize tenant admin."
  - **Spec:** `store-config`, `payment-methods`, `cash-register-terminals`

## 8. Frontend — StoreConfig (US-030)

- [ ] 8.1 `interface:entity` Entidade StoreConfig Vue (~1h)
  - **Agent:** `Frontend Entity (Vue)`
  - **Prompt:** "Entidade StoreConfig TypeScript pura com Result<T>; espelha RF-027."

- [ ] 8.2 `interface:usecase` StoreConfig use cases Vue (~1h)
  - **Agent:** `Frontend UseCase (Vue)`
  - **Prompt:** "GetStoreConfigUseCase, UpdateStoreConfigUseCase Promise<Result<T>>."

- [ ] 8.3 `interface:repository` StoreConfig repository (~1h)
  - **Agent:** `Frontend Repository (Vue)`
  - **Prompt:** "IStoreConfigRepository → /api/settings/store-config."

- [ ] 8.4 `interface:form-web` Form configuração loja (~2h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "Form /settings/store vee-validate + PrimeVue; todos campos RF-027."
  - **Spec:** `store-settings-web-ui`

## 9. Frontend — PaymentMethod & CashRegister (US-031)

- [ ] 9.1 `interface:entity` PaymentMethod + CashRegister Vue (~1h)
  - **Agent:** `Frontend Entity (Vue)`
  - **Prompt:** "Entidades PaymentMethod e CashRegisterTerminal TypeScript puras."

- [ ] 9.2 `interface:usecase` + `interface:repository` Settings lists (~3h)
  - **Agent:** `Frontend UseCase (Vue)` + `Frontend Repository (Vue)`
  - **Prompt:** "CRUD use cases + repositories payment-methods e cash-registers."

- [ ] 9.3 `interface:page` Listagens DataTable (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "Páginas /settings/payment-methods e /settings/cash-registers PrimeVue DataTable."
  - **Spec:** `store-settings-web-ui`

- [ ] 9.4 `interface:form-web` Forms cadastro (~2h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "Dialogs create/edit PaymentMethod (nome, acrescimo) e CashRegister (nome, operador dropdown)."
  - **Spec:** `store-settings-web-ui`

## 10. Tests

- [ ] 10.1 `test:unit` StoreSettings domain + app (~2h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit: StoreConfig, PaymentMethod, PaymentSurchargeCalculator, CashRegisterTerminal. Coverlet ≥95%."

- [ ] 10.2 `test:e2e` Settings API (~2h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: GET/PUT store-config; CRUD payment-method; tenant isolation 403."

## 11. Acceptance verification

- [ ] 11.1 Validar US-030: config loja RF-027 completa, form Vue, tenant scope
- [ ] 11.2 Validar US-031: formas pgto com acrescimo, caixas físicos, RN-047 calculator
- [ ] 11.3 Confirmar EP-006 pode referenciar PaymentMethodId e CashRegisterTerminalId
