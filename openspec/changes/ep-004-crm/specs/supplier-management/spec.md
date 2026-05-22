## ADDED Requirements

### Requirement: Supplier CRUD

Tenant users SHALL manage suppliers (fornecedores) with person type Physical or Legal entity.

#### Scenario: Create individual supplier

- **WHEN** operator creates supplier with PersonType Individual and valid CPF
- **THEN** supplier is persisted for tenant

#### Scenario: Create company supplier

- **WHEN** operator creates supplier with PersonType Company and valid CNPJ
- **THEN** supplier is persisted with company tax document

#### Scenario: Update supplier

- **WHEN** operator updates supplier contact and tax document
- **THEN** changes persist with validation for person type

### Requirement: Supplier list with filters

The system SHALL provide paginated supplier list filtered by name and person type.

#### Scenario: List suppliers

- **WHEN** operator requests supplier list
- **THEN** response returns paginated suppliers for current tenant only

### Requirement: Tenant isolation

Supplier operations SHALL be isolated per tenant.

#### Scenario: Cross-tenant supplier access denied

- **WHEN** user requests supplier id belonging to another tenant
- **THEN** response status is 404 or 403
