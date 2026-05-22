## Why

O MVP RetailOps (Release 1) depende de autenticação e autorização equivalentes ao legado PHP: login por email ou CPF, RBAC granular via catálogo `acessos`, bypass para SAS/Administrador, e validação de PIN de gerente no PDV. Sem o BC Identity & Access, nenhum painel tenant, operação SAS nem parallel run seguro é possível após o bootstrap (EP-000).

## What Changes

- Bootstrap auth core + backend C# com JWT, ASP.NET Core Identity adaptado e compatibilidade multi-tenant
- Modelo de domínio: `User` aggregate, `PermissionGrant`, VOs (`Email`, `Cpf`, `PasswordHash`, `UserLevel`, `ManagerPin`)
- Serviços de domínio: `AuthorizationPolicy`, `LegacyMd5PasswordVerifier`, `ManagerAuthenticationService`
- Casos de uso: login, atribuição/listagem de permissões, verificação de PIN de gerente
- Persistência: `UserEfRepository` + ACL legado (`usuarios`, `usuarios_permissoes`, `acessos`) conforme `acl-design.md`
- API: `POST /api/auth/login`, `POST /api/auth/register`, `GET /api/auth/me`, CRUD permissões, `POST /api/auth/verify-manager-pin`
- Frontend Vue: tela de login pública, guard de rotas por `PermissionKey`, gestão usuários/permissões
- Mobile Android: tela de perfil do usuário autenticado
- Testes unitários (≥95% domain/app do BC) e E2E dos fluxos críticos de auth
- Migração transparente MD5 → bcrypt no primeiro login bem-sucedido (**sem replicar MD5 em código novo permanente**)

## Capabilities

### New Capabilities

- `user-authentication`: Login JWT (email/CPF + senha), endpoint `/me`, registro trial, ACL MD5 + re-hash bcrypt
- `rbac-permissions`: Catálogo de permissões, grants por usuário, políticas Admin/SAS bypass e bloqueio sem grants
- `manager-pin-authorization`: Validação de PIN de gerente para operações de caixa (substitui senha plaintext legada)
- `auth-web-ui`: Páginas Vue (login, gestão usuários/permissões) e guards de rota
- `auth-mobile-profile`: Tela de perfil Android consumindo `/api/auth/me`

### Modified Capabilities

- `multi-tenancy`: JWT emitido pelo módulo auth passa a popular claims de tenant, nível e permissões consumidas pelo middleware EP-000

## Impact

- **Backend**: novo módulo `RetailOps.Identity` (Core/Application/Infrastructure/Api)
- **Frontend**: rotas públicas `/login` e privadas com guards; stores Pinia de sessão
- **Mobile**: feature `profile` com Retrofit autenticado
- **Banco**: migrations EF para schema auth; leitura/escrita via ACL legado durante parallel run
- **Segurança**: eliminação progressiva de MD5; PIN de gerente hasheado (não plaintext)
- **Dependência**: requer EP-000 (`bootstrap-retailops`) aplicado
