## MODIFIED Requirements

### Requirement: Tenant context middleware

The API SHALL resolve current tenant from JWT claims or request header via middleware and expose `ITenantContext`. JWT issued by the Identity module SHALL include `tenant_id`, `user_level`, and `permission_keys` claims consumed by this middleware.

#### Scenario: Tenant extracted from JWT after login

- **WHEN** an authenticated request includes a valid JWT issued by `POST /api/auth/login`
- **THEN** `ITenantContext.TenantId` and user level match JWT claims for downstream handlers

#### Scenario: Permission keys available in context

- **WHEN** authenticated request includes JWT with permission claims
- **THEN** authorization handlers can evaluate required permission keys without re-querying database on every request (with optional cache invalidation on permission change)
