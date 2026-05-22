## Why

O RetailOps é um **SaaS multi-tenant**: operadores SAS precisam cadastrar empresas, contratos e controlar trial/cobrança/bloqueio — fluxos hoje em `vendas/sas/` e `cadastrar.php`. O BC Platform é core da plataforma e pré-requisito do MVP junto com auth (EP-001): trial self-service alimenta novos tenants antes do PDV (EP-006).

## What Changes

- Novo módulo `RetailOps.Platform` (Clean Architecture: Core, Application, Infrastructure)
- Agregados `Company`, `PlatformConfig`, `Contract` com VOs (`CompanyName`, `TaxDocument`, `TrialPeriod`, `MonthlyFee`, `BlockPolicy`)
- Serviços de domínio: `TrialExpirationPolicy`, `TenantBillingPolicy`, `TenantSuspensionPolicy`
- Casos de uso: registro trial, CRUD empresas SAS, gestão contratos, emissão cobrança, suspensão por inadimplência
- ACL legado: `LegacyCompanyAdapter`, mappers `empresas`/`config`/`contratos`/`receber` tipo Empresa
- API: `POST /api/platform/trial`, CRUD empresas/contratos SAS, endpoints de cobrança/suspensão
- Frontend Vue: modal cadastro trial + painel SAS empresas (DataTable + forms) — sem mobile
- Integração com EP-001: criação de admin user no trial; evento `TenantSuspended` desativa usuários
- Testes unitários ≥95% e E2E dos fluxos trial e CRUD empresa

## Capabilities

### New Capabilities

- `trial-registration`: Cadastro trial self-service — Company + admin user, `data_pgto = hoje + dias_teste`, email único
- `platform-company-management`: CRUD empresas tenant pelo painel SAS, listagem paginada
- `tenant-contracts`: Contratos de locação de software por empresa
- `tenant-billing-suspension`: Cobrança mensalidade tenant, bloqueio inadimplente, mensagem configurável
- `platform-sas-web-ui`: UI Vue SAS (trial modal + gestão empresas/contratos)

### Modified Capabilities

- `user-authentication`: Registro trial (`POST /api/auth/register` ou rota dedicada) orquestrado pelo Platform BC criando Company + admin

## Impact

- **Backend**: módulo Platform + migrations EF; ACL dual-write/read em `empresas` durante parallel run
- **Frontend**: rotas SAS em shell Vue (scope platform / TenantId 0)
- **Identity BC**: consumidor de `TenantSuspended`; criação de user no trial
- **Finance BC** (futuro EP-007): `TenantInvoice` via ACL polimórfica `receber` — stub/interface neste épico
- **Dependências**: EP-000 bootstrap, EP-001 auth
- **Sem breaking changes** em APIs já publicadas (módulo novo)
