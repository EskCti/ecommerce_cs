## ADDED Requirements

### Requirement: Low stock detection

The system SHALL identify products where stock quantity is below StockAlertLevel per RN-021.

#### Scenario: Product below alert level

- **WHEN** product stock is 3 and alert level is 5
- **THEN** product appears in low stock query results

#### Scenario: Product at or above alert level excluded

- **WHEN** product stock equals alert level
- **THEN** product is not listed as low stock

### Requirement: Low stock query API

The system SHALL expose paginated low stock products endpoint per RF-044.

#### Scenario: List low stock products

- **WHEN** manager requests low stock list
- **THEN** response returns products below threshold for current tenant

### Requirement: Low stock domain event

When stock crosses below alert level, the system MAY publish LowStockDetected event.

#### Scenario: Event on threshold cross

- **WHEN** stock adjustment causes quantity to fall below alert level
- **THEN** LowStockDetected event is published once per crossing
