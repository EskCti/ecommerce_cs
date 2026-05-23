## Context

**RetailOps** migra o PDV SaaS PHP para C#/Vue/Android. EP-000 entrega infraestrutura; EP-001 entrega Identity & Access. EP-002 implementa **BC Platform** — gestão SaaS (tenants, trial, contratos, cobrança/bloqueio).

Referências:
- `docs/modeling/loja-php/ddd-tactical-model.md` (BC Platform)
- `docs/migration/loja-php/acl-design.md` (BC Platform section)
- `docs/discovery/ecommerce-legado-php/requirements.md` (RF-002, RF-010, RF-014, RF-016, RN-003, RN-010, RN-011)

Legado: painel SAS (`vendas/sas/`), cadastro trial (`cadastrar.php`), tabelas `empresas`, `config` (empresa=0), `contratos`, `receber` tipo Empresa.

## Goals / Non-Goals

**Goals:**

- Paridade MVP: trial self-service + CRUD empresas/contratos SAS
- `TrialExpirationPolicy` alinhada a RN-003 (teste=Sim, data_pgto expirada → desativar)
- Cobrança tenant e suspensão por inadimplência (US-022 Should — incluir no design, implementar no apply)
- ACL `LegacyCompanyAdapter` para parallel run
- UI Vue painel SAS (sem mobile)
- Evento `TenantSuspended` → desativar usuários via integração Identity

**Non-Goals:**

- Gateway de pagamento online (legado não tinha — stub de cobrança/registro `receber` only)
- Relatórios SAS (EP-009 Reporting)
- Migração final de schema `platform_*` (EP-011 cutover)
- Gestão de config por tenant (EP-003 Store Settings)

## Decisions

### 1. Módulo Platform

**Decisão:** `Config New Module (C#)` cria `RetailOps.Platform.*` com referências a Shared.Kernel e Identity (apenas application ports, não referência circular de domínio).

**Integração Identity:** `IUserProvisioningPort` (interface em Platform application) implementada por adapter que chama Identity use case para criar admin no trial.

### 2. Company aggregate

**Decisão:** `Company` AR com lifecycle: `RegisterTrial()`, `Activate()`, `Suspend()`, `UpdateBillingDate()`, `AddContract()`.

**TenantId:** `CompanyId` mapeia a `empresas.id`; após criação trial, admin user recebe `TenantId = CompanyId`.

### 3. PlatformConfig

**Decisão:** Singleton lógico carregado de `config` onde `empresa=0`: `dias_teste`, `dias_bloqueio`, `msg_bloqueio`.

### 4. Trial registration flow

**Decisão:** `POST /api/platform/trial` (público, rate-limited) executa transação:
1. Validar email único
2. Criar Company (teste=Sim, data_pgto = today + dias_teste)
3. Provisionar admin user via Identity port
4. Publicar `TrialRegistered`

**Alternativa:** reutilizar `POST /api/auth/register` — **delegar** internamente a `RegisterTrialCompanyUseCase` (modified capability em auth spec).

### 5. SAS company management

**Decisão:** Endpoints sob `/api/platform/companies` exigem JWT SAS level; CQRS `ListCompaniesQuery` para DataTable Vue.

### 6. Billing and suspension

**Decisão:** `IssueTenantInvoiceUseCase` cria registro via ACL `receber` tipo Empresa; `SuspendOverdueTenantUseCase` aplica `TenantSuspensionPolicy` usando `dias_bloqueio`; handler publica `TenantSuspended` → Identity desativa users.

**Finance BC:** interface `ITenantInvoicePort` em Platform; implementação ACL agora, Finance BC absorve em EP-007.

### 7. Frontend SAS

**Decisão:** Rotas `/sas/companies`, `/sas/companies/:id/contracts`; trial modal em rota pública ou landing; PrimeVue DataTable + vee-validate forms.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Dual-write `empresas` vs EF | `IPlatformLegacyPort` único; testes integração |
| Trial spam | rate limit + captcha hook (future) |
| Circular dependency Platform↔Identity | ports na application layer apenas |
| Cobrança polimórfica `receber` | ACL isolada; não modelar no aggregate Company |
| US-022 Should vs Must | Implementar no mesmo change; marcar tasks como fase 2 no apply se necessário |

## Migration Plan

1. Módulo Platform domain → infra → API
2. Trial público com pilot flag EP-000
3. SAS panel para operadores internos
4. Parallel run: writes via ACL até cutover EP-011

**Rollback:** flag desliga `POST /api/platform/trial`; SAS continua no PHP.

## Open Questions

- Email de boas-vindas trial? **Adiar (EP-010 Notifications)**
- Job agendado suspensão vs manual SAS? **Job diário + endpoint manual trigger no MVP**
