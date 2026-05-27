## ADDED Requirements

### Requirement: Configure grade dimensions

Managers SHALL configure up to two optional grade dimensions (e.g., Color, Size) per product per RF-033.

#### Scenario: Add grade dimension

- **WHEN** manager adds dimension "Cor" to product
- **THEN** dimension is stored under Product aggregate

#### Scenario: Remove grade dimension

- **WHEN** manager removes dimension without sold stock constraints
- **THEN** dimension and options are removed

### Requirement: Grade options with stock

Each grade option SHALL have label and stock quantity per combination per RF-033/034.

#### Scenario: Add grade option with stock

- **WHEN** manager adds option "P" under size dimension with stock 10
- **THEN** option persists with StockQuantity 10

#### Scenario: Grade stock adjustment

- **WHEN** stock movement affects graded product
- **THEN** corresponding GradeOption stock and GradeMovementDetail are updated per RN-031

### Requirement: Grade editor API

The system SHALL expose API to configure dimensions and options for a product.

#### Scenario: Load grade tree

- **WHEN** manager requests product grade configuration
- **THEN** response returns dimensions with nested options and stock levels
