# Tasks — ep-004-crm (EP-004 CRM)

Referência: `docs/planning/loja-php/backlog.md` · US-040, US-041

## 1. Module setup

- [x] 1.1 `infra:setup` Módulo Crm (~1h)
  - **Agent:** `Config New Module (C#)`
  - **Prompt:** "Crie RetailOps.Crm: Core, Application, Infrastructure; tenant-scoped; ports ICustomerRepository, ISupplierRepository."
  - **Spec:** `customer-management`

## 2. Domain — Customer (US-040)

- [x] 2.1 `domain:vo` Person VOs (~2h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "PersonName, Cpf, Email, Phone, Address, CustomerId com Create() Result<T>; reutilizar shared onde existir."
  - **Spec:** `customer-management`

- [x] 2.2 `domain:entity` Customer + Attachment (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Customer AR com Attachment child; UpdateContactInfo, AddAttachment, Deactivate."
  - **Spec:** `customer-management`

- [x] 2.3 `domain:service` CustomerRegistrationPolicy (~1h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Valida CPF único por tenant; regras cadastro cliente."
  - **Spec:** `customer-management`

## 3. Domain — Supplier (US-041)

- [x] 3.1 `domain:vo` SupplierId, PersonType, TaxDocument (~1h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "PersonType Individual|Company; TaxDocument CPF/CNPJ conforme tipo."
  - **Spec:** `supplier-management`

- [x] 3.2 `domain:entity` Supplier aggregate (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Supplier AR: contatos, PersonType, TaxDocument, Deactivate()."
  - **Spec:** `supplier-management`

## 4. Application layer

- [x] 4.1 `app:usecase` Customer CRUD + FindOrCreateByCpf (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "CreateCustomer, UpdateCustomer, DeactivateCustomer, FindOrCreateByCpfUseCase; eventos CustomerRegistered/CustomerFoundByCpf."
  - **Spec:** `customer-management`

- [x] 4.2 `app:usecase` Supplier CRUD (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Create, Update, Deactivate Supplier use cases."
  - **Spec:** `supplier-management`

- [x] 4.3 `app:query` ListCustomersQuery, ListSuppliersQuery (~2h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "Queries paginadas filtros nome, cpf, personType; projeções DTO."
  - **Spec:** `customer-management`, `supplier-management`

## 5. Infrastructure

- [x] 5.1 `infra:persistence` CRM Legacy ACL (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyCustomerMapper, LegacySupplierMapper, ICrmLegacyPort; tabelas clientes, fornecedores, arquivos tipo Cliente."
  - **Spec:** `customer-management`, `supplier-management`

## 6. API

- [x] 6.1 `interface:controller` CrmCustomersController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "CRUD /api/crm/customers; POST find-or-create-by-cpf; attachments sub-routes."
  - **Spec:** `customer-management`

- [x] 6.2 `interface:controller` CrmSuppliersController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "CRUD /api/crm/suppliers; tenant scope; permission policies."
  - **Spec:** `supplier-management`

## 7. Frontend full-stack — Customer (US-040)

- [x] 7.1 `interface:entity` Entidade Customer Vue (~1h)
  - **Agent:** `Frontend Entity (Vue)`
  - **Prompt:** "Entidade Customer TypeScript pura com Result<T>; validações CPF."

- [x] 7.2 `interface:usecase` Customer use cases Vue (~2h)
  - **Agent:** `Frontend UseCase (Vue)`
  - **Prompt:** "CreateCustomer, UpdateCustomer, ListCustomers, FindOrCreateByCpf use cases."

- [x] 7.3 `interface:repository` Customer repository Vue (~2h)
  - **Agent:** `Frontend Repository (Vue)`
  - **Prompt:** "ICustomerRepository → /api/crm/customers; map DTO→entity."

- [x] 7.4 `interface:page` Listagem clientes (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "Página /crm/customers PrimeVue DataTable, filtros CPF/nome, paginação."
  - **Spec:** `crm-web-ui`

- [x] 7.5 `interface:form-web` Form cliente (~2h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "Form cadastro/edição cliente vee-validate + PrimeVue; campos RF-020."
  - **Spec:** `crm-web-ui`

## 8. Frontend — Supplier (US-041)

- [x] 8.1 `interface:entity` + `interface:usecase` + `interface:repository` Supplier Vue (~4h)
  - **Agent:** `Frontend Entity (Vue)` + `Frontend UseCase (Vue)` + `Frontend Repository (Vue)`
  - **Prompt:** "Stack completa Supplier consumindo /api/crm/suppliers."

- [x] 8.2 `interface:page` + `interface:form-web` Fornecedores (~3h)
  - **Agent:** `Frontend Page (Vue)` + `Frontend Form (Vue)`
  - **Prompt:** "Listagem + form fornecedor; seletor pessoa F/J; RF-021."
  - **Spec:** `crm-web-ui`

## 9. Mobile — Customer (US-040)

- [x] 9.1 `interface:mobile-entity` Customer Android (~1h)
  - **Agent:** `Mobile Entity (Android)`
  - **Prompt:** "data class Customer Kotlin puro + sealed Result."

- [x] 9.2 `interface:mobile-usecase` + `interface:mobile-repository` (~3h)
  - **Agent:** `Mobile UseCase (Android)` + `Mobile Repository (Android)`
  - **Prompt:** "ListCustomers + FindOrCreateByCpf; Retrofit /api/crm/customers."

- [x] 9.3 `interface:mobile` + `interface:mobile-form` Telas Compose (~4h)
  - **Agent:** `Mobile Screen (Android)` + `Mobile Form (Android)`
  - **Prompt:** "CustomerListScreen LazyColumn + CustomerFormScreen; pull-to-refresh."
  - **Spec:** `crm-mobile-customers`

## 10. Tests

- [x] 10.1 `test:unit` CRM domain + app (~2h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit: CustomerRegistrationPolicy, FindOrCreateByCpf, Supplier CRUD. Coverlet ≥95%."

- [x] 10.2 `test:e2e` CRM API (~2h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: CRUD customer; find-or-create idempotente; tenant isolation."

## 11. Acceptance verification

- [x] 11.1 Validar US-040: CRUD cliente, FindOrCreateByCpf, Vue + Android, RF-020
- [x] 11.2 Validar US-041: CRUD fornecedor F/J, Vue, RF-021
- [x] 11.3 Documentar contrato CustomerId para EP-006 Sales
