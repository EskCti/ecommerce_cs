# Tasks — bootstrap-retailops (EP-000)

Referência: `docs/planning/loja-php/backlog.md` · US-000 + US-001

## 1. Solution infrastructure (US-000)

- [ ] 1.1 `infra:setup` Solução C# multi-projeto (~2h)
  - **Agent:** `Config Project (C#)`
  - **Prompt:** "Crie solução RetailOps.sln: RetailOps.Shared.Kernel, RetailOps.Core, RetailOps.Infrastructure, RetailOps.Api, UnitTests + IntegrationTests. Clean Architecture. Endpoint GET /health."
  - **Spec:** `solution-infrastructure`

- [ ] 1.2 `test:e2e` Smoke test health (~1h)
  - **Agent:** `E2E Tests (C#)`
  - **Prompt:** "IntegrationTests: GET /health retorna 200."
  - **Spec:** `solution-infrastructure`

## 2. Shared kernel (US-000)

- [ ] 2.1 `domain:shared` Shared kernel C# (~2h)
  - **Agent:** `Config Shared Core (C#)`
  - **Prompt:** "Entity base, Result<T>, TenantId, Money, Email, IUseCase, IRepository, TransactionManager."
  - **Spec:** `shared-kernel`

## 3. Database infrastructure (US-000)

- [ ] 3.1 `infra:db` EF Core + docker-compose dev (~2h)
  - **Agent:** `Config EF Core (C#)`
  - **Prompt:** "DbContext base, migrations vazias, ConnectionString, docker-compose com Postgres compatível schema sas."
  - **Spec:** `database-infrastructure`

## 4. Legacy data ACL (US-000)

- [ ] 4.1 `infra:migration` ACL base LegacySasDbContext (~2h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "RetailOps.Infrastructure.Legacy: DbContext mapeando tabelas sas.sql; sem vazar nomes legado ao domínio."
  - **Spec:** `legacy-data-acl`

## 5. Web admin shell (US-000)

- [ ] 5.1 `infra:setup` Bootstrap Vue web-vue only (~2h)
  - **Agent:** `Config Project (Vue)`
  - **Prompt:** "Bootstrap somente apps/web-vue (Vue 3 + PrimeVue 4 + Pinia + Router). Sem NestJS. Proxy Vite /api → http://localhost:5000."
  - **Spec:** `web-admin-shell`

- [ ] 5.2 `infra:shell-web` Shell Vue PrimeVue (~2h)
  - **Agent:** `Config Shared Web (Vue)`
  - **Prompt:** "Shell admin: sidebar colapsável, topbar, rodapé, rotas tenant + platform."
  - **Spec:** `web-admin-shell`

## 6. Mobile Android bootstrap (US-000)

- [ ] 6.1 `infra:setup` Bootstrap Android (~2h)
  - **Agent:** `Config Project (Android)`
  - **Prompt:** "App em apps/mobile-android: Kotlin Compose + Hilt + Retrofit; base URL configurável (default 10.0.2.2:5000)."
  - **Spec:** `mobile-android-bootstrap`

## 7. Deployment pipeline (US-000)

- [ ] 7.1 `infra:docker` Dockerfiles produção (~1h)
  - **Agent:** `Config Docker (C#)`
  - **Prompt:** "Multi-stage Dockerfile API + docker-compose.prod.yml."
  - **Spec:** `deployment-pipeline`

- [ ] 7.2 `infra:cicd` GitHub Actions (~2h)
  - **Agent:** `Config CI/CD (C#)`
  - **Prompt:** "CI: dotnet test + coverlet ≥95% domain/app. CD: build/push Docker main."
  - **Spec:** `deployment-pipeline`

## 8. Multi-tenancy and migration flags (US-001)

- [ ] 8.1 `domain:vo` TenantId VO (~1h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "TenantId value object; 0 = platform scope."
  - **Spec:** `multi-tenancy`

- [ ] 8.2 `app:usecase` TenantContext middleware (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "ITenantContext + middleware ASP.NET Core; integrar com JWT claims."
  - **Spec:** `multi-tenancy`

- [ ] 8.3 `interface:controller` Config endpoint Migration (~1h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "GET /api/admin/migration-status (SAS only)."
  - **Spec:** `multi-tenancy`

## 9. Acceptance verification

- [ ] 9.1 Validar critérios US-000: build, /health, shell Vue, Android compile, docker compose, CI verde
- [ ] 9.2 Validar critérios US-001: TenantId, middleware, flags pilot, endpoint migration-status
