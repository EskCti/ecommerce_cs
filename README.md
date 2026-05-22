# RetailOps

Reimplementação do PDV SaaS legado (PHP `sas`) em Clean Architecture: **ASP.NET Core 8**, **Vue 3 + PrimeVue**, **Android Kotlin Compose**.

## Estrutura (monorepo `apps/`)

| Caminho | Descrição |
| ------- | --------- |
| `apps/backend/RetailOps.Api` | API HTTP (`GET /health`, admin migration) |
| `apps/backend/RetailOps.Core` | Domínio / contratos de aplicação |
| `apps/backend/RetailOps.Infrastructure` | EF Core (Postgres), multi-tenancy |
| `apps/backend/RetailOps.Infrastructure.Legacy` | ACL leitura schema `sas` |
| `apps/backend/RetailOps.Shared.Kernel` | Entity, Result, VOs, IUseCase |
| `apps/backend/tests/` | Unit + integration tests |
| `apps/web-vue` | Admin Vue 3 (proxy `/api` → `:5000`) |
| `apps/mobile-android` | App Android Compose |

`RetailOps.sln` na raiz referencia os projetos em `apps/backend/`.

## Desenvolvimento local

```bash
cp .env.example .env
docker compose up -d
dotnet run --project apps/backend/RetailOps.Api
cd apps/web-vue && npm install && npm run dev
```

- API: http://localhost:5000/health  
- Web: http://localhost:5173  

## Testes

```bash
dotnet test
```

## OpenSpec

Change: `openspec/changes/bootstrap-retailops` (EP-000). Orquestração: **Config Project Full-Stack** → `Config Project (C#)` + Vue + Android.
