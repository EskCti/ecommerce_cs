## MODIFIED Requirements

### Requirement: Migration pilot flags

Configuration SHALL support Migration pilot and module rollout flags during strangler migration. At cutover completion, all active tenants SHALL route exclusively to RetailOps with LegacyPhpEnabled false and no per-BC legacy fallback.

#### Scenario: Non-pilot tenant stays on legacy path

- **WHEN** tenant ID is not in pilot list for a given BC flag during migration phase
- **THEN** migration routing treats the tenant as legacy-only for that BC

#### Scenario: All tenants fully migrated at cutover

- **WHEN** EP-011 cutover completes
- **THEN** all active tenants use C# implementation for all bounded contexts with no pilot list exceptions

#### Scenario: Migration status reflects full cutover

- **WHEN** platform admin calls GET /api/admin/migration-status after cutover
- **THEN** response shows LegacyPhpEnabled false and AllTenantsOnRetailOps true
