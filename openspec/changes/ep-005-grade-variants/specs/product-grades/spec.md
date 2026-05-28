## ADDED Requirements

### Requirement: Grade variant entity

The system SHALL model stock-holding combinations as GradeVariant entities under the Product aggregate, each linking one option per configured dimension (1 or 2 options).

#### Scenario: Single-dimension variant auto-created

- **WHEN** manager adds option "Cx" under sole dimension "Embalagem"
- **THEN** system creates GradeVariant linking that option with initial stock from input

#### Scenario: Two-dimension cartesian sync

- **WHEN** product has dimensions "Cor" (Azul, Vermelho) and "Tamanho" (P, M)
- **THEN** system maintains four GradeVariants (Azul/P, Azul/M, Vermelho/P, Vermelho/M) each with independent stock

#### Scenario: Maximum two dimensions

- **WHEN** manager attempts to add a third dimension
- **THEN** operation fails with validation error

### Requirement: Variant stock adjustment

Managers SHALL adjust stock per GradeVariant via API and UI matrix editor per RF-034.

#### Scenario: Adjust variant stock in matrix

- **WHEN** manager sets stock 5 for variant Azul + P
- **THEN** only that variant stock updates; other combinations unchanged

#### Scenario: Remove option removes variants

- **WHEN** manager removes option "P" from Tamanho dimension
- **THEN** all variants referencing that option are removed

## MODIFIED Requirements

### Requirement: Configure grade dimensions

Managers SHALL configure up to two optional grade dimensions (e.g., Color, Size) per product per RF-033.

#### Scenario: Add grade dimension

- **WHEN** manager adds dimension "Cor" to product
- **THEN** dimension is stored under Product aggregate

#### Scenario: Remove grade dimension

- **WHEN** manager removes dimension without sold stock constraints
- **THEN** dimension, its options, and dependent GradeVariants are removed

### Requirement: Grade options with stock

Each grade option SHALL define labels per dimension; stock quantity SHALL be held on GradeVariant per combination when two dimensions exist, or on the single-dimension GradeVariant when only one dimension exists per RF-033/034.

#### Scenario: Add grade option with stock (1 dimension)

- **WHEN** manager adds option "Cx" under sole dimension with stock 10
- **THEN** option persists and corresponding GradeVariant has StockQuantity 10

#### Scenario: Add grade option without per-option stock (2 dimensions)

- **WHEN** manager adds option "G" under Tamanho on a product that already has Cor dimension
- **THEN** new GradeVariants are created for G combined with each Cor option, each with stock 0 until set in matrix

#### Scenario: Grade stock adjustment

- **WHEN** stock movement or matrix edit affects a graded product combination
- **THEN** corresponding GradeVariant stock and GradeMovementDetail are updated per RN-031

### Requirement: Grade editor API

The system SHALL expose API to configure dimensions, options, and variant stock matrix for a product.

#### Scenario: Load grade tree

- **WHEN** manager requests product grade configuration
- **THEN** response returns dimensions with nested options and GradeVariants with stock levels and combination labels
