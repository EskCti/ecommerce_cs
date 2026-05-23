# Tasks — ep-002-platform (EP-002 Platform SaaS)

Referência: `docs/planning/loja-php/backlog.md` · US-020, US-021, US-022

## 1. Module setup (US-021)

- [x] 1.1 `infra:setup` Módulo Platform (~1h)
  - **Agent:** `Config New Module (C#)`
  - **Prompt:** "Crie módulo RetailOps.Platform: Core, Application, Infrastructure, controllers; referências Shared.Kernel; sem circular deps com Identity."
  - **Spec:** `platform-company-management`

## 2. Domain — trial (US-020)

- [x] 2.1 `domain:entity` Company, PlatformConfig (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Company AR: RegisterTrial(), Suspend(), UpdateBillingDate(). PlatformConfig singleton dias_teste, dias_bloqueio, msg_bloqueio."
  - **Spec:** `trial-registration`, `tenant-billing-suspension`

- [x] 2.2 `domain:service` TrialExpirationPolicy (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "TrialExpirationPolicy RN-003: teste=Sim e data_pgto < hoje → inactive."
  - **Spec:** `trial-registration`

## 3. Application — trial (US-020)

- [x] 3.1 `app:usecase` RegisterTrialCompanyUseCase (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Transação: valida email único, cria Company trial, provisiona admin via IUserProvisioningPort, publica TrialRegistered."
  - **Spec:** `trial-registration`, `user-authentication`

## 4. Infrastructure — trial (US-020)

- [x] 4.1 `infra:persistence` LegacyCompanyAdapter (~2h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "LegacyCompanyAdapter + LegacyCompanyMapper + LegacyPlatformConfigMapper conforme acl-design.md."
  - **Spec:** `trial-registration`, `platform-company-management`

## 5. API — trial (US-020)

- [x] 5.1 `interface:controller` POST /api/platform/trial (~1h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "Endpoint público rate-limited; delega RegisterTrialCompanyUseCase; retorna tenant id."
  - **Spec:** `trial-registration`

## 6. Frontend — trial (US-020)

- [x] 6.1 `interface:form-web` Modal cadastro trial Vue (~2h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "Form público trial: empresa + admin; vee-validate; POST /api/platform/trial; redirect login."
  - **Spec:** `platform-sas-web-ui`, `trial-registration`

## 7. Domain — companies & contracts (US-021)

- [x] 7.1 `domain:entity` Company, Contract (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "Contract entity filha de Company; AddContract(), UpdateCompanyDetails()."
  - **Spec:** `platform-company-management`, `tenant-contracts`

## 8. Application — companies (US-021)

- [x] 8.1 `app:usecase` CreateCompany, UpdateCompany, SaveContract (~4h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "CreateCompany, UpdateCompany, SaveContract use cases; SAS scope only."
  - **Spec:** `platform-company-management`, `tenant-contracts`

- [x] 8.2 `app:query` ListCompaniesQuery (~1h)
  - **Agent:** `Core Query CQRS (C#)`
  - **Prompt:** "ListCompaniesQuery paginada com filtros nome, ativo, teste; projeção DTO."
  - **Spec:** `platform-company-management`

## 9. API — companies (US-021)

- [x] 9.1 `interface:controller` PlatformCompaniesController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "CRUD /api/platform/companies; contratos nested; authorize SAS level."
  - **Spec:** `platform-company-management`, `tenant-contracts`

## 10. Frontend full-stack — Company SAS (US-021)

- [x] 10.1 `interface:entity` Entidade frontend Company (~1h)
  - **Agent:** `Frontend Entity (Vue)`
  - **Prompt:** "Entidade Company TypeScript pura com Result<T>; espelha domínio Platform."

- [x] 10.2 `interface:usecase` Use cases frontend Company (~2h)
  - **Agent:** `Frontend UseCase (Vue)`
  - **Prompt:** "CreateCompanyUseCase, UpdateCompanyUseCase, ListCompaniesUseCase com Promise<Result<T>>."

- [x] 10.3 `interface:repository` HttpRepository Company (~2h)
  - **Agent:** `Frontend Repository (Vue)`
  - **Prompt:** "ICompanyRepository consumindo /api/platform/companies; map DTO→entity."

- [x] 10.4 `interface:page` Listagem empresas SAS (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "Página /sas/companies PrimeVue DataTable, filtros, paginação."
  - **Spec:** `platform-sas-web-ui`

- [x] 10.5 `interface:form-web` Form empresa + contratos (~2h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "Form cadastro/edição Company + tab contratos; vee-validate + PrimeVue."
  - **Spec:** `platform-sas-web-ui`, `tenant-contracts`

## 11. Billing and suspension (US-022)

- [x] 11.1 `domain:service` TenantBillingPolicy, TenantSuspensionPolicy (~3h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "TenantBillingPolicy RN-010; TenantSuspensionPolicy RN-011 com dias_bloqueio e msg_bloqueio."
  - **Spec:** `tenant-billing-suspension`

- [x] 11.2 `app:usecase` IssueTenantInvoice, SuspendOverdueTenant (~3h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "IssueTenantInvoiceUseCase via ITenantInvoicePort ACL receber; SuspendOverdueTenantUseCase publica TenantSuspended."
  - **Spec:** `tenant-billing-suspension`

- [x] 11.3 Event handler TenantSuspended → deactivate users (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Handler TenantSuspended chama Identity port DeactivateUsersByTenant; idempotente."
  - **Spec:** `tenant-billing-suspension`, `user-authentication`

## 12. Tests

- [x] 12.1 `test:unit` Platform domain + application (~2h)
  - **Agent:** `Unit Tests (C#)`
  - **Prompt:** "xUnit + Moq: TrialExpirationPolicy, RegisterTrialCompany, SuspendOverdue. Coverlet ≥95% Platform.Core/Application."

- [x] 12.2 `test:e2e` Trial + company CRUD (~2h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "WebApplicationFactory: POST /api/platform/trial; SAS CRUD company; 403 tenant user."

## 13. Acceptance verification

- [x] 13.1 Validar US-020: trial cria company+admin, email único, data_pgto, form Vue
- [x] 13.2 Validar US-021: CRUD SAS, contratos, listagem Vue
- [x] 13.3 Validar US-022: cobrança, suspensão, desativação usuários, msg bloqueio
