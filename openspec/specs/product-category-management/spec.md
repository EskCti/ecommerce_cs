## ADDED Requirements

### Requirement: Product CRUD

Tenant users SHALL create, read, update, deactivate, and delete products with barcode, pricing, stock, and category.

#### Scenario: Create product with unique barcode

- **WHEN** manager creates product with barcode not used in tenant
- **THEN** product is persisted with generated ProductId

#### Scenario: Duplicate barcode rejected

- **WHEN** manager creates product with barcode already used in tenant
- **THEN** response status is 409 or validation error

#### Scenario: Open price product

- **WHEN** manager sets sale price to zero per RN-020
- **THEN** product is marked OpenPrice and PDV may prompt price at sale time

### Requirement: Category CRUD

The system SHALL manage product categories with active/inactive status per RF-032.

#### Scenario: Create active category

- **WHEN** manager creates category with active status
- **THEN** category appears in product assignment dropdown

### Requirement: Auto-generate barcode

The system SHALL generate unique product barcode codes per tenant per RF-031.

#### Scenario: Generate barcode

- **WHEN** manager requests auto-generated barcode for new product
- **THEN** system returns unused barcode value for tenant

### Requirement: Product photo whitelist

Product photo upload or path SHALL accept only whitelisted file extensions from legacy rules.

#### Scenario: Invalid photo extension rejected

- **WHEN** manager submits photo with disallowed extension
- **THEN** validation error is returned without saving photo path

### Requirement: Profit margin on purchase update

When cost price changes, the system SHALL recalculate profit margin percentage per RN-022.

#### Scenario: Margin recalculated

- **WHEN** cost price and sale price are updated on purchase
- **THEN** stored profit margin reflects `(sale - cost) / cost * 100`
