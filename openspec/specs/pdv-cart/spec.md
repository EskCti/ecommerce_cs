## ADDED Requirements

### Requirement: Scan product by barcode

Operators SHALL add products to cart by barcode scan or manual entry per RF-051.

#### Scenario: Add product by barcode

- **WHEN** operator scans valid product barcode with sufficient stock
- **THEN** SaleLine is added to current CashSession cart

#### Scenario: Quantity prefix scan

- **WHEN** operator scans barcode prefixed with quantity marker (legacy 2* pattern)
- **THEN** line quantity reflects parsed prefix value

#### Scenario: Insufficient stock blocked

- **WHEN** requested quantity exceeds available stock for non-open-price product per RN-045
- **THEN** add item fails with validation error

### Requirement: Confirm grade for cart line

For graded products, operators SHALL confirm grade selection before line is sellable per RF-052.

#### Scenario: Grade confirmation required

- **WHEN** graded product is added without grade selection
- **THEN** line remains pending grade confirmation until operator selects options

#### Scenario: Grade confirmed

- **WHEN** operator confirms valid grade combination with stock
- **THEN** line becomes ready for finalization and stock is reserved

### Requirement: Cart stock reservation

Adding or confirming cart lines SHALL reserve stock via Catalog integration until sale completes or line is removed.

#### Scenario: Reservation on confirmed line

- **WHEN** cart line is confirmed with quantity 2
- **THEN** Catalog reserves 2 units until release or finalize

#### Scenario: Release on line removal

- **WHEN** operator removes cart line
- **THEN** reserved stock is released
