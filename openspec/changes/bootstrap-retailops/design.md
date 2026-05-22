## Context

Repositório **greenfield** com documentação de discovery/modeling/migration/planning para reimplementar o PDV SaaS legado PHP (`exemplos/php/vendas/`, schema MariaDB `sas`) como **RetailOps**. Stack definida: **ASP.NET Core 8**, **Vue 3 + PrimeVue**, **Android Kotlin Compose**. Branch alvo: `feat/ep-000-bootstrap-infra-tech`.

Estado atual: apenas `docs/`, `.env.example`, submodule de skills — sem solução .NET, frontend ou mobile.

## Goals / Non-Goals

**Goals:**

- Monorepo compilável com Clean Architecture por camada (Shared.Kernel → Core → Infrastructure → Api)
- Shared kernel reutilizável por todos os BCs futuros
- Postgres dev via docker-compose; ACL de leitura do schema legado sem contaminar domínio
- Shell web admin e app Android apontando para API local (`localhost:5000` / `10.0.2.2:5000`)
- CI com `dotnet test` + Coverlet; CD com build Docker na `main`
- Multi-tenancy base + flags de migração (Strangler Fig) conforme `migration-strategy.md`
- Smoke E2E `GET /health` → 200

**Non-Goals:**

- Autenticação JWT/RBAC (EP-001)
- Implementação de bounded contexts de negócio (catálogo, PDV, financeiro)
- Cutover ou desligamento do PHP (EP-011)
- B2C storefront online

## Decisions

### 1. Estrutura de solução C#

**Decisão:** `RetailOps.sln` com projetos `RetailOps.Shared.Kernel`, `RetailOps.Core`, `RetailOps.Infrastructure`, `RetailOps.Api`, `RetailOps.UnitTests`, `RetailOps.IntegrationTests`.

**Alternativas:** projeto único (rejeitado — impede modularização por BC); pastas por BC sem projetos separados ainda (adiado — EP-000 usa camadas horizontais vazias).

**Rationale:** Alinha a `config-project-cs` e permite crescimento modular por BC em EP-001+.

### 2. Banco de dados dev

**Decisão:** Postgres 16 no `docker-compose.yml` dev; EF Core como ORM principal; projeto `RetailOps.Infrastructure.Legacy` para ACL read-only do schema `sas`.

**Alternativas:** MariaDB idêntico ao legado (compatível, mas Postgres já está no `.env.example`); Dapper-only (rejeitado para domínio novo).

**Rationale:** Postgres simplifica dev local; ACL isolada permite parallel run sem modelar `receber` polimórfico no domínio.

### 3. Frontend Vue sem NestJS

**Decisão:** `apps/web-vue` standalone; proxy Vite `/api` → `http://localhost:5000`; shell via `config-shared-web-vue`.

**Alternativas:** monorepo NestJS+Vue do skill padrão (rejeitado — backend é C#).

**Rationale:** Combinação documentada em `dotnet-angular-android` adaptada para Vue.

### 4. Mobile Android

**Decisão:** `apps/mobile-android` com Compose, Hilt, Retrofit; base URL configurável via `BuildConfig` / `local.properties`.

**Rationale:** Stack do backlog; emulador usa `10.0.2.2:5000`.

### 5. Multi-tenancy

**Decisão:** `ITenantContext` + middleware ASP.NET Core; `TenantId` VO (0 = platform/SAS); config `Migration:PilotTenantIds` por BC.

**Rationale:** Suporta Strangler Fig e isolamento antes de EP-001 auth completo.

### 6. CI/CD

**Decisão:** GitHub Actions — job `test` (restore, build, test, Coverlet threshold domain/app); job `docker` em push `main`.

**Rationale:** RNF-001, RNF-006 do discovery.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| ACL legado expõe nomes de tabela ao domínio | Projeto `Infrastructure.Legacy` separado; mappers explícitos; domínio nunca referencia Legacy |
| Vue skill assume NestJS | Bootstrap manual de `web-vue` only; proxy para porta 5000 |
| Coverlet ≥95% cedo demais | Threshold aplicado a Shared.Kernel/Core vazios inicialmente; ampliar com BCs |
| Postgres ≠ MariaDB legado | Tipos EF testados contra `sas.sql`; pilot tenant em parallel run |
| Android exige Android SDK local | Documentar `./gradlew assembleDebug`; CI Android opcional em fase posterior |

## Migration Plan

1. Bootstrap infra (este change) — branch `feat/ep-000-bootstrap-infra-tech`
2. Merge após CI verde e critérios US-000/US-001 atendidos
3. EP-001 auth sobre shared kernel existente
4. BCs migrados incrementalmente com flags `Migration:PilotTenantIds`

**Rollback:** remover containers Docker; branch revert; PHP legado permanece operacional (parallel run).

## Open Questions

- Registry Docker para CD (GitHub Container Registry vs outro) — definir secrets no primeiro deploy
- CI Android no bootstrap ou adiar para EP com telas mobile — **adiar CI Android** no EP-000
