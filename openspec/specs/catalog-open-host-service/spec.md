## ADDED Requirements

### Requirement: Find product by barcode

The catalog SHALL expose lookup by barcode returning product, pricing, open-price flag, grades, and available stock for Sales BC.

#### Scenario: Barcode lookup success

- **WHEN** Sales BC requests product by valid tenant barcode
- **THEN** response includes product id, name, sale price, open price flag, stock, and grade options

#### Scenario: Unknown barcode

- **WHEN** barcode does not exist for tenant
- **THEN** response status is 404

### Requirement: Stock reservation port

The catalog SHALL provide reserve and release stock operations for Sales cart lifecycle.

#### Scenario: Reserve stock for cart line

- **WHEN** Sales requests reservation of quantity 2 for product
- **THEN** available stock decreases by 2 until release or sale confirmation

#### Scenario: Release reservation

- **WHEN** Sales releases reservation on cart item removal
- **THEN** available stock is restored

### Requirement: Parallel run stock read

For pilot tenants, catalog SHALL support comparing stock quantities against legacy ACL read model.

#### Scenario: Stock comparison report

- **WHEN** admin triggers parallel run stock compare for pilot tenant
- **THEN** report lists products with quantity mismatch between new and legacy systems
