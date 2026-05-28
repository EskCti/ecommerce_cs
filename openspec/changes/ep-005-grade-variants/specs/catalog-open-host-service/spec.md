## MODIFIED Requirements

### Requirement: Find product by barcode

The catalog SHALL expose lookup by barcode returning product, pricing, open-price flag, grade dimensions, grade variants with stock, and available stock for Sales BC.

#### Scenario: Barcode lookup success

- **WHEN** Sales BC requests product by valid tenant barcode
- **THEN** response includes product id, name, sale price, open price flag, grade dimensions, grade variants (optionIds, stock), and aggregate stock for non-graded products

#### Scenario: Unknown barcode

- **WHEN** barcode does not exist for tenant
- **THEN** response status is 404

### Requirement: Stock reservation port

The catalog SHALL provide reserve and release stock operations by product (non-graded) or by GradeVariant (graded) for Sales cart lifecycle.

#### Scenario: Reserve stock for non-graded cart line

- **WHEN** Sales requests reservation of quantity 2 for product without grades
- **THEN** available product stock decreases by 2 until release or sale confirmation

#### Scenario: Reserve stock for graded variant

- **WHEN** Sales requests reservation of quantity 2 for GradeVariant Azul + P
- **THEN** that variant available stock decreases by 2 until release or sale confirmation

#### Scenario: Release reservation

- **WHEN** Sales releases reservation on cart item removal
- **THEN** available stock is restored on product or GradeVariant as applicable

## ADDED Requirements

### Requirement: Resolve variant by option selection

The catalog SHALL resolve a GradeVariant from ordered grade option ids for PDV confirmation.

#### Scenario: Resolve two-option variant

- **WHEN** Sales submits option ids [AzulId, PId] for a two-dimension product
- **THEN** catalog returns matching GradeVariant id and available stock

#### Scenario: Invalid combination

- **WHEN** option ids do not form a valid variant for the product
- **THEN** response status is 400 with validation error
