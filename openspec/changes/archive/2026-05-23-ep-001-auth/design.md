## Context

**RetailOps** reimplementa o PDV SaaS legado PHP com stack C# + Vue + Android. EP-000 entrega bootstrap (solução, shared kernel, EF Core, shell Vue, multi-tenancy base). EP-001 implementa o **Bounded Context Identity & Access** — subdomínio genérico de identidade conforme `docs/modeling/loja-php/ddd-tactical-model.md`.

Regras legado relevantes (`requirements.md`):
- RF-001: login email ou CPF + senha
- RF-006/RF-007: permissões granulares via `usuarios_permissoes` + catálogo `acessos`
- RN-001–RN-004: MD5 legado, bloqueio sem permissões, trial expirado, níveis SAS/Administrador/Gerente/Operador/Vendedor

ACL detalhada em `docs/migration/loja-php/acl-design.md` (Identity section).

## Goals / Non-Goals

**Goals:**

- Paridade funcional de auth/RBAC com legado para MVP Release 1
- JWT com claims de `TenantId`, `UserLevel` e permissões efetivas
- ACL de leitura/escrita em tabelas legado durante parallel run
- Re-hash bcrypt transparente no primeiro login MD5 válido
- UI Vue: login + administração de permissões + route guards
- Android: perfil do usuário logado
- Cobertura de testes ≥95% domain/application do BC; E2E dos fluxos de login

**Non-Goals:**

- OAuth/social login (fora do legado)
- Gestão SAS de empresas/trial (EP-002 Platform — `POST /api/auth/register` trial pode delegar a EP-002 posteriormente; stub mínimo aceitável se necessário para login)
- Permissões dinâmicas além do catálogo `acessos` legado (35 itens seed)
- Biometria mobile

## Decisions

### 1. Módulo e camadas

**Decisão:** Pacote `RetailOps.Identity` com projetos Core (domain+application), Infrastructure (EF + Legacy ACL), exposto via controllers em `RetailOps.Api`.

**Alternativas:** ASP.NET Identity puro sem domínio (rejeitado — viola Clean Architecture); auth dentro de Platform BC (rejeitado — BC separado no modelo).

### 2. Autenticação JWT

**Decisão:** JWT Bearer emitido por `AuthenticateUserUseCase`; claims: `sub`, `tenant_id`, `user_level`, `permission_keys[]`; validação via middleware ASP.NET Core + integração com `ITenantContext` (EP-000).

**Alternativas:** cookies de sessão (rejeitado — SPA Vue + mobile Retrofit preferem Bearer).

### 3. Senha legado MD5

**Decisão:** `ILegacyMd5PasswordVerifier` na infraestrutura; evento `PasswordRehashRequested` → persistir bcrypt na mesma transação após login OK.

**Alternativas:** migração batch offline (adiada — Strangler exige login incremental).

### 4. Autorização RBAC

**Decisão:** `AuthorizationPolicy` no domínio:
- `IsPrivileged(UserLevel)` → SAS e Administrador bypass
- `RequiresGrants()` → demais níveis exigem ≥1 `PermissionGrant`
- API policies `[Authorize(Policy = "Permission:chave")]` mapeadas a `PermissionKey`

**Alternativas:** roles fixas apenas (rejeitado — legado usa grants granulares).

### 5. PIN de gerente

**Decisão:** `ManagerPin` VO separado de senha de login; `ManagerAuthenticationService` compara hash (bcrypt); endpoint dedicado `POST /api/auth/verify-manager-pin`.

**Rationale:** RN legado misturava `senha` plaintext — reimplementação corrige RNF-004.

### 6. Frontend Vue

**Decisão:** Pinia store `useAuthStore` (token, user, permissions); router `beforeEach` guard; PrimeVue forms para login e gestão de permissões.

### 7. Mobile Android

**Decisão:** Feature `profile` apenas (US-011 template); token em DataStore; interceptor Retrofit adiciona Bearer.

## Risks / Trade-offs

| Risco | Mitigação |
|-------|-----------|
| Dual-write auth legado + EF durante parallel run | `IUserLegacyPort` único ponto de escrita; testes de integração |
| JWT sem refresh token | Access token TTL configurável; re-login aceitável no MVP |
| Catálogo `acessos` desatualizado vs PHP | Seed idempotente dos 35 itens; diff test contra `sas.sql` |
| Guard Vue dessincronizado da API | `/me` como fonte de verdade pós-login |
| Trial expirado verificado só no login | `AuthenticateUserUseCase` consulta status empresa via ACL |

## Migration Plan

1. Implementar domain → application → infra → API → Vue → Android → tests
2. Pilot tenant com flag EP-000 roteia login para RetailOps API
3. Monitorar re-hash bcrypt e taxa de falhas MD5
4. Quando 100% tenants auth migrados (EP-011), remover `LegacyMd5PasswordVerifier`

**Rollback:** desabilitar pilot flag; tráfego volta ao PHP `autenticar.php`.

## Open Questions

- Refresh token no Release 2? **Adiar**
- `POST /api/auth/register` trial implementado aqui ou EP-002? **Stub delegando a Platform quando EP-002 existir; login trial user criado manualmente no MVP**
