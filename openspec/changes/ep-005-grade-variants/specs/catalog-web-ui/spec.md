## MODIFIED Requirements

### Requirement: Grade editor UI

The Vue app SHALL provide grade dimension editor and variant stock matrix for products per US-051.

#### Scenario: Configure single-dimension grades in UI

- **WHEN** manager adds dimension "Embalagem" and option "Cx" with stock 20 on a one-dimension product
- **THEN** UI saves via grades API and displays variant stock 20

#### Scenario: Configure two-dimension matrix in UI

- **WHEN** manager configures Cor (Azul, Vermelho) and Tamanho (P, M) on a product
- **THEN** UI displays 2×2 matrix grid where manager sets stock per cell (combination) and saves via variant stock API

#### Scenario: Matrix reflects API variants

- **WHEN** manager reopens grade editor after save
- **THEN** matrix cells match GradeVariant stock returned by API

## ADDED Requirements

### Requirement: Grade variant labels in UI

The grade editor SHALL display human-readable combination labels (e.g., "Azul / P") derived from dimension option labels.

#### Scenario: Variant label shown in matrix

- **WHEN** matrix loads for Cor × Tamanho product
- **THEN** each cell header shows combined option labels
