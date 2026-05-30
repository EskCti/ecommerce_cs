## ADDED Requirements

### Requirement: Manual stock entry

Operators SHALL record manual stock entries with reason and quantity per RF-040.

#### Scenario: Stock entry increases quantity

- **WHEN** operator records entry of 5 units with reason "Inventário"
- **THEN** product stock increases by 5 and StockMovement audit is created

### Requirement: Manual stock exit

Operators SHALL record manual stock exits with reason and quantity per RF-041.

#### Scenario: Stock exit decreases quantity

- **WHEN** operator records exit of 3 units
- **THEN** product stock decreases by 3 without going negative

#### Scenario: Insufficient stock rejected

- **WHEN** exit quantity exceeds available stock
- **THEN** operation fails with validation error

### Requirement: Stock purchase

The system SHALL process product purchase updating cost/stock and publishing ProductPurchased event per RF-042. Finance BC SHALL consume ProductPurchased to create PurchasePayable with correct PaymentStatus per RN-061.

#### Scenario: Purchase updates inventory

- **WHEN** operator completes stock purchase with quantity and cost
- **THEN** stock and cost price update, ProductPurchased event is published, and Finance handler creates purchase payable

#### Scenario: Future due purchase payable open

- **WHEN** purchase event includes due date after today
- **THEN** Finance creates payable with PaymentStatus Open per RN-061

### Requirement: Stock movement audit

All adjustments SHALL be recorded as StockMovement aggregate with MovementType.

#### Scenario: Movement history query

- **WHEN** manager requests movement history for product
- **THEN** paginated StockMovement list is returned for tenant
