# Tasks — ep-001-auth (EP-001 Identity & Access)

Referência: `docs/planning/loja-php/backlog.md` · US-010, US-011, US-012

> **Legenda**: tasks **6.x–8.x MVP** entregaram UI com `fetch` na store (aceitável como protótipo). **12–14** completam Clean Architecture e testes front/mobile conforme `req-agile-planning`.

## 1. Auth infrastructure bootstrap (US-010)

- [x] 1.1 `infra:auth` Bootstrap auth core + backend (~3h)
  - **Agent:** `Config Auth Core (C#)` + `Config Auth Backend Basic (C#)`
  - **Prompt:** "Auth full RBAC: User, Permission, JWT, ASP.NET Identity adaptado. Endpoints register/login/me. Compatível multi-tenant."
  - **Spec:** `user-authentication`, `rbac-permissions`

## 2. Domain layer — authentication (US-010)

- [x] 2.1 `domain:vo` Email, Cpf, PasswordHash, UserLevel (~2h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "VOs com Create() Result<T>. UserLevel enum: Sas, Administrador, Gerente, Operador, Vendedor."
  - **Spec:** `user-authentication`

- [x] 2.2 `domain:entity` User aggregate + PermissionGrant (~2h)
  - **Agent:** `Core Entity (C#)`
  - **Prompt:** "User AR com AssignPermission(), Deactivate(). PermissionGrant entity filha."
  - **Spec:** `user-authentication`, `rbac-permissions`

- [x] 2.3 `domain:service` AuthorizationPolicy, LegacyMd5Verifier (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Admin/SAS bypass; RequiresGrants(); ILegacyMd5PasswordVerifier."
  - **Spec:** `user-authentication`, `rbac-permissions`

## 3. Application layer — authentication (US-010)

- [x] 3.1 `app:usecase` AuthenticateUserUseCase (~2h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "Login email/CPF; ACL usuarios; emite JWT; dispara PasswordRehashRequested."
  - **Spec:** `user-authentication`, `multi-tenancy`

## 4. Infrastructure — authentication (US-010)

- [x] 4.1 `infra:persistence` UserEfRepository + LegacyUserAdapter (~3h)
  - **Agent:** `Backend Data (C#)`
  - **Prompt:** "UserEfRepository + LegacyUserMapper conforme acl-design.md."
  - **Spec:** `user-authentication`

## 5. API — authentication (US-010)

- [x] 5.1 `interface:controller` AuthController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "POST /api/auth/login, POST /api/auth/register (trial), GET /api/auth/me."
  - **Spec:** `user-authentication`

## 6. Frontend — login MVP (US-010) — superseded by §12

- [x] 6.1 `interface:page` Tela login Vue — MVP store+fetch (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "Página pública login; armazena JWT; redireciona tenant vs SAS por level."
  - **Spec:** `auth-web-ui`
  - **Nota:** Refinar em 12.4 após entity/usecase/repository.

## 7. Tests — authentication API (US-010)

- [x] 7.1 `test:unit` + `test:e2e` Auth API (~3h)
  - **Agent:** `Unit Tests (C#)` + `E2E Tests (C#)`
  - **Prompt:** "Testes login válido/inválido/trial expirado/sem permissão."
  - **Spec:** `user-authentication`

## 8. RBAC — permissions backend + MVP UI (US-011)

- [x] 8.1 `app:usecase` AssignPermissionsUseCase, ListPermissionsQuery (~2h)
  - **Agent:** `Core Use Case (C#)` + `Core Query CQRS (C#)`
  - **Prompt:** "AssignPermissionsUseCase revoga/atribui grants; ListPermissionsQuery paginada por tenant."
  - **Spec:** `rbac-permissions`

- [x] 8.2 `interface:controller` UsersPermissionsController (~2h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "CRUD permissões por usuário; seed catálogo acessos; policies PermissionKey."
  - **Spec:** `rbac-permissions`

- [x] 8.3 `interface:page` + `interface:form-web` Gestão usuários — MVP (~4h)
  - **Agent:** `Frontend Page (Vue)` + `Frontend Form (Vue)`
  - **Prompt:** "Listagem usuários DataTable; form atribuição permissões; route guard PermissionKey."
  - **Spec:** `auth-web-ui`, `rbac-permissions`
  - **Nota:** Refinar em 12.5–12.7 após camadas de domínio Vue.

## 9. Mobile — profile MVP (US-011) — harden in §13

- [x] 9.1 `interface:mobile-entity` Entidade User Android (~1h)
  - **Agent:** `Mobile Entity (Android)`
  - **Prompt:** "data class User Kotlin puro + sealed Result; espelha /api/auth/me."

- [x] 9.2 `interface:mobile-usecase` GetCurrentUserUseCase (~1h)
  - **Agent:** `Mobile UseCase (Android)`
  - **Prompt:** "GetCurrentUserUseCase suspend consumindo /api/auth/me."

- [x] 9.3 `interface:mobile-repository` Retrofit auth (~2h)
  - **Agent:** `Mobile Repository (Android)`
  - **Prompt:** "IAuthRepository + Retrofit; Bearer interceptor; map DTO→User."

- [x] 9.4 `interface:mobile` Tela perfil Compose (~2h)
  - **Agent:** `Mobile Screen (Android)`
  - **Prompt:** "ProfileScreen Compose + ViewModel StateFlow; logout em 401."
  - **Spec:** `auth-mobile-profile`

## 10. Manager PIN (US-012)

- [x] 10.1 `domain:vo` ManagerPin (~1h)
  - **Agent:** `Core Value Object (C#)`
  - **Prompt:** "ManagerPin VO com hash bcrypt; separado de PasswordHash."
  - **Spec:** `manager-pin-authorization`

- [x] 10.2 `domain:service` ManagerAuthenticationService (~2h)
  - **Agent:** `Core Domain Service (C#)`
  - **Prompt:** "Valida PIN Gerente/Administrador; rejeita Operador/Vendedor."
  - **Spec:** `manager-pin-authorization`

- [x] 10.3 `app:usecase` VerifyManagerPinUseCase (~1h)
  - **Agent:** `Core Use Case (C#)`
  - **Prompt:** "VerifyManagerPinUseCase; integra ACL senha legado na migração."
  - **Spec:** `manager-pin-authorization`

- [x] 10.4 `interface:controller` POST /api/auth/verify-manager-pin (~1h)
  - **Agent:** `Backend Controller (C#)`
  - **Prompt:** "Endpoint verify-manager-pin; JWT obrigatório; rate limit básico."
  - **Spec:** `manager-pin-authorization`

## 12. Frontend Vue — Clean Architecture (US-010 / US-011)

- [x] 12.1 `interface:entity` AuthUser + TenantUser Vue (~2h)
  - **Agent:** `Frontend Entity (Vue)`
  - **Prompt:** "apps/web-vue/src/modules/auth/domain e modules/users/domain: entidades com Result<T>; sem Vue/Pinia/fetch."
  - **Spec:** `auth-web-ui`, `rbac-permissions`

- [x] 12.2 `interface:usecase` Auth + Users use cases Vue (~3h)
  - **Agent:** `Frontend UseCase (Vue)`
  - **Prompt:** "LoginUseCase, RestoreSessionUseCase, ListUsersUseCase, AssignPermissionsUseCase; Promise<Result<T>>; IAuthRepository/IUsersPermissionsRepository."
  - **Spec:** `auth-web-ui`, `rbac-permissions`

- [x] 12.3 `interface:repository` AuthHttpRepository + UsersPermissionsHttpRepository (~3h)
  - **Agent:** `Frontend Repository (Vue)`
  - **Prompt:** "Implementar repositórios HTTP; map DTO→entity; Bearer do token; endpoints /api/auth/* e /api/users/*."
  - **Spec:** `auth-web-ui`, `rbac-permissions`

- [x] 12.4 `interface:page` Refatorar LoginView (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "LoginView usa LoginUseCase; Pinia auth.store só token/sessão e delega use cases."
  - **Spec:** `auth-web-ui`

- [x] 12.5 `interface:page` Refatorar UsersPermissionsView (~2h)
  - **Agent:** `Frontend Page (Vue)`
  - **Prompt:** "DataTable via ListUsersUseCase; sem fetch na view."
  - **Spec:** `rbac-permissions`

- [x] 12.6 `interface:form-web` Form permissões Vue (~2h)
  - **Agent:** `Frontend Form (Vue)`
  - **Prompt:** "MultiSelect + AssignPermissionsUseCase; validação inline de seleção vazia."
  - **Spec:** `rbac-permissions`

## 13. Mobile Android — CA hardening (US-011)

- [x] 13.1 `interface:mobile-entity` User em domain/repository (~1h)
  - **Agent:** `Mobile Entity (Android)`
  - **Prompt:** "Mover User para profile/domain; IAuthRepository em profile/domain/repository; Result sealed class."

- [x] 13.2 `interface:mobile-repository` AuthRepositoryImpl + DataStore token (~2h)
  - **Agent:** `Mobile Repository (Android)`
  - **Prompt:** "Repository impl em data/; AuthTokenHolder→DataStore; interceptor Bearer; 401 limpa token."

- [x] 13.3 `interface:mobile` ProfileScreen logout 401 (~1h)
  - **Agent:** `Mobile Screen (Android)`
  - **Prompt:** "ViewModel trata Failure Unauthorized; navegação/estado logged out."

## 14. Tests — frontend e mobile (US-010 / US-011)

- [x] 14.1 `test:unit-web` Vitest auth Vue (~2h)
  - **Agent:** `Frontend UseCase (Vue)`
  - **Prompt:** "vitest em apps/web-vue; testes LoginUseCase e repositórios com fetch mock."
  - **Spec:** `auth-web-ui`

- [x] 14.2 `test:unit-mobile` JUnit GetCurrentUserUseCase (~1h)
  - **Agent:** `Mobile UseCase (Android)`
  - **Prompt:** "src/test/java: MockK IAuthRepository; Success e Failure."
  - **Spec:** `auth-mobile-profile`

## 11. Acceptance verification

- [ ] 11.1 Validar US-010: JWT login, 403 cenários, MD5→bcrypt, `/me`, testes API + Vue use cases
- [ ] 11.2 Validar US-011: CRUD permissões, seed 35 acessos, guards Vue, perfil Android com CA
- [ ] 11.3 Validar US-012: verify-manager-pin com PIN hasheado
