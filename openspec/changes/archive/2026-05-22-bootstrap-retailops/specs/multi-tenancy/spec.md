## ADDED Requirements

### Requirement: TenantId value object

The domain SHALL define `TenantId` with validation; value `0` SHALL represent platform (SAS) scope.

#### Scenario: Invalid tenant rejected

- **WHEN** `TenantId.Create` receives an invalid value
- **THEN** it returns a failed `Result` without creating the value object

### Requirement: Tenant context middleware

The API SHALL resolve current tenant from JWT claims or request header via middleware and expose `ITenantContext`.

#### Scenario: Tenant extracted from request

- **WHEN** an authenticated request includes a valid tenant claim
- **THEN** `ITenantContext.TenantId` matches the claim for downstream handlers

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
