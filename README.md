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

### Opção A — Docker Compose (stack completa)

```bash
cp .env.example .env
docker compose up -d --build
```

- API: http://localhost:5000/health  
- Web: http://localhost:5173 (proxy `/api` → API no container)

On first run in Development, the API creates the minimal legacy schema in Postgres automatically. Use **Cadastro trial** at `/register-trial` to create your first tenant admin, then log in with that e-mail.

### Opção B — Processos locais (hot reload)

```bash
cp .env.example .env
docker compose up -d db          # só Postgres
dotnet run --project apps/backend/RetailOps.Api
cd apps/web-vue && npm install && npm run dev
```

- API: http://localhost:5000/health  
- Web: http://localhost:5173  

### Produção (local)

```bash
cp .env.example .env   # defina Jwt__Secret
docker compose -f docker-compose.prod.yml up -d --build
```

- Web + API: http://localhost:8080 (nginx faz proxy de `/api` para a API)

## Testes

```bash
dotnet test
cd apps/web-vue && npm run test:unit
cd apps/mobile-android && ./gradlew assembleDebug
```

## CI/CD

| Workflow | Escopo |
| -------- | ------ |
| `ci.yml` | Backend (`dotnet test`), Vue (`build` + `vitest`), Android (`assembleDebug`), smoke `docker build` API + Web |
| `cd.yml` | Push imagens `retailops-api` e `retailops-web` para GHCR; artefato APK Android |

## OpenSpec

Change: `openspec/changes/bootstrap-retailops` (EP-000). Orquestração: **Config Project Full-Stack** → `Config Project (C#)` + Vue + Android.
