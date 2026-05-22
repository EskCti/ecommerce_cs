# Tasks — ep-008-returns (EP-008 Returns)

Referência: `docs/planning/loja-php/backlog.md` · US-080 (+ RF-061)

## 1. Module setup

- [ ] 1.1 `infra:setup` Módulo Returns (~1h)
  - **Agent:** `Config New Module (C#)`
  - **Prompt:** "Crie RetailOps.Returns: Core, Application, Infrastructure; IExchangeRepository; ports CRM e Catalog stock."
  - **Spec:** `product-exchange`

## 2. Domain layer (US-080)

- [ ] 2.1 `domain:vo` ExchangeId, ExchangeQuantity, GradeSelection (~1h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "ExchangeQuantity invariante = 1 RN-050; GradeSelection optional 2 dims."
  - **Spec:** `product-exchange`

- [ ] 2.2 `domain:entity` Exchange aggregate (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Exchange AR: CustomerId, ProductIn/Out, grades; Register() valida estoque saída."
  - **Spec:** `product-exchange`

- [ ] 2.3 `domain:service` ExchangeStockPolicy (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Delega IStockLegacyPort +1 entrada -1 saída; Troca Entrada/Saída grade details."
  - **Spec:** `product-exchange`, `product-grades`

## 3. Application layer

- [ ] 3.1 `app:usecase` RegisterExchangeUseCase (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "RegisterExchange RF-060; ICustomerProvisioningPort FindOrCreateByCpf RN-051; ProductExchanged event."
  - **Spec:** `product-exchange`, `customer-management`

- [ ] 3.2 `app:usecase` DeleteExchangeUseCase (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "DeleteExchange RF-061; reverte estoque ±1."
  - **Spec:** `exchange-management`

- [ ] 3.3 `app:query` ListExchangesQuery (~1h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ListExchangesQuery paginada filtros data, cliente, produto."
  - **Spec:** `exchange-management`

## 4. Infrastructure

- [ ] 4.1 `infra:persistence` LegacyExchangeAdapter (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyExchangeMapper, LegacyExchangeAdapter, IReturnsLegacyPort; trocas + detalhes_grade ACL."
  - **Spec:** `returns-legacy-acl`

## 5. API

- [ ] 5.1 `interface:controller` ReturnsExchangesController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "POST/GET/DELETE /api/returns/exchanges; tenant scope; RBAC."
  - **Spec:** `product-exchange`, `exchange-management`

## 6. Frontend Vue (US-080)

- [ ] 6.1 `interface:entity` Exchange Vue (~1h)
  - **Agent:** `Frontend Entity (Vue)`
  - **Prompt:** "Entidade Exchange TypeScript pura Result<T>; validação RN-050."

- [ ] 6.2 `interface:usecase` + `interface:repository` Returns (~2h)
  - **Agent:** `Frontend UseCase (Vue)` + `Frontend Repository (Vue)`
  - **Prompt:** "RegisterExchange, ListExchanges, DeleteExchange; /api/returns/exchanges."

- [ ] 6.3 `interface:form-web` Form troca (~3h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "Form troca: CPF cliente, produto entrada/saída, grades, vee-validate + PrimeVue."
  - **Spec:** `returns-web-ui`

- [ ] 6.4 `interface:page` Listagem trocas (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "/returns/exchanges DataTable RF-061; delete com confirmação."
  - **Spec:** `returns-web-ui`

## 7. Tests

- [ ] 7.1 `test:unit` Returns domain (~2h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit: Exchange RN-050, ExchangeStockPolicy mocks. Coverlet ≥95%."

- [ ] 7.2 `test:e2e` Exchange flow (~2h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: register exchange → stock adjusted; delete reverses; CPF auto-create mock."

## 8. Acceptance verification

- [ ] 8.1 Validar US-080: troca 1:1, grades, estoque, RN-051, form Vue
- [ ] 8.2 Validar RF-061: listagem e exclusão com reversão estoque
- [ ] 8.3 Confirmar integração Catalog grade movements e CRM FindOrCreateByCpf
