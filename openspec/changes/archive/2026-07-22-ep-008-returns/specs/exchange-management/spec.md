## ADDED Requirements

### Requirement: List exchanges

Authorized users SHALL list registered exchanges for tenant per RF-061.

#### Scenario: Paginated exchange list

- **WHEN** user requests exchanges list with date filter
- **THEN** paginated exchanges with customer and product summaries are returned

### Requirement: Delete exchange

Authorized users SHALL delete exchange records per RF-061.

#### Scenario: Delete exchange reverses stock

- **WHEN** user deletes exchange record
- **THEN** exchange is removed and stock adjustments are reversed (+1 out product, -1 in product)

#### Scenario: Unauthorized delete rejected

- **WHEN** user without permission attempts delete
- **THEN** response status is 403

### Requirement: Tenant isolation

Exchange queries and mutations SHALL be scoped to current tenant.

#### Scenario: Cross-tenant exchange not accessible

- **WHEN** user requests exchange id from another tenant
- **THEN** response status is 404 or 403
