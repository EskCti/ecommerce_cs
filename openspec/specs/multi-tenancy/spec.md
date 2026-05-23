# multi-tenancy Specification

## Purpose

Resolve and expose tenant context per request for the RetailOps multi-tenant SaaS, including JWT claims from Identity and migration pilot routing.

## Requirements

### Requirement: TenantId value object

The domain SHALL define `TenantId` with validation; value `0` SHALL represent platform (SAS) scope.

#### Scenario: Invalid tenant rejected

- **WHEN** `TenantId.Create` receives an invalid value
- **THEN** it returns a failed `Result` without creating the value object

### Requirement: Tenant context middleware

The API SHALL resolve current tenant from JWT claims or request header via middleware and expose `ITenantContext`. JWT issued by the Identity module SHALL include `tenant_id`, `user_level`, and `permission_keys` claims consumed by this middleware.

#### Scenario: Tenant extracted from JWT after login

- **WHEN** an authenticated request includes a valid JWT issued by `POST /api/auth/login`
- **THEN** `ITenantContext.TenantId` and user level match JWT claims for downstream handlers

#### Scenario: Permission keys available in context

- **WHEN** authenticated request includes JWT with permission claims
- **THEN** authorization handlers can evaluate required permission keys without re-querying database on every request (with optional cache invalidation on permission change)

### Requirement: Migration pilot flags

Configuration SHALL support `Migration:PilotTenantIds` (or equivalent) listing tenant IDs allowed to route to new implementation per bounded context.

#### Scenario: Non-pilot tenant stays on legacy path

- **WHEN** tenant ID is not in pilot list for a given BC flag
- **THEN** migration routing treats the tenant as legacy-only for that BC

### Requirement: Migration status endpoint

The API SHALL expose `GET /api/admin/migration-status` restricted to SAS/platform scope.

#### Scenario: SAS admin reads migration flags

- **WHEN** platform-scoped admin calls `GET /api/admin/migration-status`
- **THEN** response includes current pilot tenant IDs and BC rollout flags
