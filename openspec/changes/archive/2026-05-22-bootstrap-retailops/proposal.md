## Why

O legado PHP (`vendas/`, schema `sas`) precisa ser reimplementado como **RetailOps** em Clean Architecture com entrega contínua desde o início. Sem bootstrap full-stack (C#, Vue, Android, Docker, CI/CD e shared kernel), nenhum bounded context do backlog pode ser implementado com segurança nem com parallel run contra o PHP.

## What Changes

- Criar solução .NET multi-projeto `RetailOps.sln` com API ASP.NET Core e endpoint `/health`
- Instalar shared kernel C# (`Result<T>`, `Entity`, VOs base, `IUseCase`, `IRepository`, `TransactionManager`)
- Configurar EF Core, `DbContext` base, migrations iniciais e `docker-compose` dev (Postgres compatível com schema legado)
- Adicionar Dockerfiles de produção e pipeline GitHub Actions (test + build/push de imagem)
- Bootstrap frontend Vue 3 + PrimeVue com shell admin (sidebar, topbar, rodapé)
- Bootstrap app Android Kotlin Compose + Hilt + Retrofit apontando para API local
- Criar ACL `LegacySasDbContext` mapeando tabelas `sas.sql` sem expor nomes legados ao domínio
- Implementar multi-tenancy base e feature flags de migração (Strangler Fig)
- Smoke test E2E: `GET /health` retorna 200

## Capabilities

### New Capabilities

- `solution-infrastructure`: Solução C# Clean Architecture, projetos de teste, API com `/health`
- `shared-kernel`: Tipos base de domínio compartilhados (Entity, Result, contratos de use case/repositório)
- `database-infrastructure`: EF Core, migrations, connection strings e Postgres via docker-compose dev
- `deployment-pipeline`: Docker multi-stage e GitHub Actions (CI test/coverage + CD imagem)
- `web-admin-shell`: App Vue 3 + PrimeVue com layout admin e proxy para API C#
- `mobile-android-bootstrap`: App Android Compose consumindo API com URL configurável
- `legacy-data-acl`: DbContext de leitura do schema legado `sas` isolado na infraestrutura
- `multi-tenancy`: Contexto de tenant, middleware e flags de cutover por BC

### Modified Capabilities

_(nenhuma — repositório greenfield sem specs OpenSpec prévias)_

## Impact

- **Código novo**: `apps/backend/RetailOps.*`, `apps/backend/tests/`, `apps/web-vue/`, `apps/mobile-android/`
- **Infra**: `docker-compose.yml`, `docker-compose.prod.yml`, `.github/workflows/`
- **Config**: `.env.example` estendido com connection strings e flags de migração
- **Documentação**: alinhado a `docs/planning/loja-php/backlog.md` (EP-000) e `docs/migration/loja-php/`
- **Sem breaking changes** em APIs públicas (projeto novo)
