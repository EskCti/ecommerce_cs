# Tasks — ep-001-auth (EP-001 Identity & Access)

Referência: `docs/planning/loja-php/backlog.md` · US-010, US-011, US-012

## 1. Auth infrastructure bootstrap (US-010)

- [ ] 1.1 `infra:auth` Bootstrap auth core + backend (~3h)
  - **Agent:** `Config Auth Core (C#)` + `Config Auth Backend Basic (C#)`
  - **Prompt:** "Auth full RBAC: User, Permission, JWT, ASP.NET Identity adaptado. Endpoints register/login/me. Compatível multi-tenant."
  - **Spec:** `user-authentication`, `rbac-permissions`

## 2. Domain layer — authentication (US-010)

- [ ] 2.1 `domain:vo` Email, Cpf, PasswordHash, UserLevel (~2h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "VOs com Create() Result<T>. UserLevel enum: Sas, Administrador, Gerente, Operador, Vendedor."
  - **Spec:** `user-authentication`

- [ ] 2.2 `domain:entity` User aggregate + PermissionGrant (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "User AR com AssignPermission(), Deactivate(). PermissionGrant entity filha."
  - **Spec:** `user-authentication`, `rbac-permissions`

- [ ] 2.3 `domain:service` AuthorizationPolicy, LegacyMd5Verifier (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Admin/SAS bypass; RequiresGrants(); ILegacyMd5PasswordVerifier."
  - **Spec:** `user-authentication`, `rbac-permissions`

## 3. Application layer — authentication (US-010)

- [ ] 3.1 `app:usecase` AuthenticateUserUseCase (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Login email/CPF; ACL usuarios; emite JWT; dispara PasswordRehashRequested."
  - **Spec:** `user-authentication`, `multi-tenancy`

## 4. Infrastructure — authentication (US-010)

- [ ] 4.1 `infra:persistence` UserEfRepository + LegacyUserAdapter (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "UserEfRepository + LegacyUserMapper conforme acl-design.md."
  - **Spec:** `user-authentication`

## 5. API — authentication (US-010)

- [ ] 5.1 `interface:controller` AuthController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "POST /api/auth/login, POST /api/auth/register (trial), GET /api/auth/me."
  - **Spec:** `user-authentication`

## 6. Frontend — login (US-010)

- [ ] 6.1 `interface:page` Tela login Vue (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "Página pública login; armazena JWT; redireciona tenant vs SAS por level."
  - **Spec:** `auth-web-ui`

## 7. Tests — authentication (US-010)

- [ ] 7.1 `test:unit` + `test:e2e` Auth (~3h)
  - **Agent:** `Unit Tests (C#)` + `E2E Tests (C#)`
  - **Prompt:** "Testes login válido/inválido/trial expirado/sem permissão."
  - **Spec:** `user-authentication`

## 8. RBAC — permissions (US-011)

- [ ] 8.1 `app:usecase` AssignPermissionsUseCase, ListPermissionsQuery (~2h)
  - **Agent:** `Core Use Case (C#)` + `Core Query CQRS (C#)`
  - **Prompt:** "AssignPermissionsUseCase revoga/atribui grants; ListPermissionsQuery paginada por tenant."
  - **Spec:** `rbac-permissions`

- [ ] 8.2 `interface:controller` UsersPermissionsController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "CRUD permissões por usuário; seed catálogo acessos; policies PermissionKey."
  - **Spec:** `rbac-permissions`

- [ ] 8.3 `interface:page` + `interface:form-web` Gestão usuários/permissões Vue (~4h)
  - **Agent:** `Frontend Page (Vue)` + `Frontend Form (Vue)`
  - **Prompt:** "Listagem usuários DataTable; form atribuição permissões; route guard PermissionKey."
  - **Spec:** `auth-web-ui`, `rbac-permissions`

## 9. Mobile — user profile (US-011)

- [ ] 9.1 `interface:mobile-entity` Entidade User Android (~1h)
  - **Agent:** `Mobile Entity (Android)`
  - **Prompt:** "data class User Kotlin puro + sealed Result; espelha /api/auth/me."

- [ ] 9.2 `interface:mobile-usecase` GetCurrentUserUseCase (~1h)
  - **Agent:** `Mobile UseCase (Android)`
  - **Prompt:** "GetCurrentUserUseCase suspend consumindo /api/auth/me."

- [ ] 9.3 `interface:mobile-repository` Retrofit auth (~2h)
  - **Agent:** `Mobile Repository (Android)`
  - **Prompt:** "IAuthRepository + Retrofit; Bearer interceptor; map DTO→User."

- [ ] 9.4 `interface:mobile` Tela perfil Compose (~2h)
  - **Agent:** `Mobile Screen (Android)`
  - **Prompt:** "ProfileScreen Compose + ViewModel StateFlow; logout em 401."
  - **Spec:** `auth-mobile-profile`

## 10. Manager PIN (US-012)

- [ ] 10.1 `domain:vo` ManagerPin (~1h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "ManagerPin VO com hash bcrypt; separado de PasswordHash."
  - **Spec:** `manager-pin-authorization`

- [ ] 10.2 `domain:service` ManagerAuthenticationService (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Valida PIN Gerente/Administrador; rejeita Operador/Vendedor."
  - **Spec:** `manager-pin-authorization`

- [ ] 10.3 `app:usecase` VerifyManagerPinUseCase (~1h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "VerifyManagerPinUseCase; integra ACL senha legado na migração."
  - **Spec:** `manager-pin-authorization`

- [ ] 10.4 `interface:controller` POST /api/auth/verify-manager-pin (~1h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "Endpoint verify-manager-pin; JWT obrigatório; rate limit básico."
  - **Spec:** `manager-pin-authorization`

## 11. Acceptance verification

- [ ] 11.1 Validar US-010: JWT login, 403 cenários, MD5→bcrypt, `/me`, testes E2E
- [ ] 11.2 Validar US-011: CRUD permissões, seed 35 acessos, guards Vue, perfil Android
- [ ] 11.3 Validar US-012: verify-manager-pin com PIN hasheado
