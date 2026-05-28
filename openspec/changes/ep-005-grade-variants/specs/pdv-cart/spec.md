## MODIFIED Requirements

### Requirement: Confirm grade for cart line

For graded products, operators SHALL confirm grade selection by choosing options per dimension (1 or 2) or by GradeVariantId before line is sellable per RF-052.

#### Scenario: Grade confirmation required

- **WHEN** graded product is added without grade selection
- **THEN** line remains pending grade confirmation until operator selects required options or variant

#### Scenario: Single-dimension grade confirmed

- **WHEN** operator confirms one option on a single-dimension graded product with sufficient variant stock
- **THEN** line becomes ready for finalization and stock is reserved on the matching GradeVariant

#### Scenario: Two-dimension grade confirmed

- **WHEN** operator confirms Cor "Azul" and Tamanho "P" with sufficient stock on variant Azul + P
- **THEN** line becomes ready for finalization and stock is reserved on that GradeVariant only

### Requirement: Cart stock reservation

Adding or confirming cart lines SHALL reserve stock via Catalog integration on Product (non-graded) or GradeVariant (graded) until sale completes or line is removed.

#### Scenario: Reservation on confirmed variant line

- **WHEN** cart line is confirmed with quantity 2 on GradeVariant Azul + P
- **THEN** Catalog reserves 2 units on that variant until release or finalize

#### Scenario: Release on line removal

- **WHEN** operator removes cart line that reserved a GradeVariant
- **THEN** reserved variant stock is released

## ADDED Requirements

### Requirement: Two-step grade picker in PDV UI

The Vue and Android PDV UI SHALL guide operators through dimension selection when product has two grade dimensions.

#### Scenario: Vue PDV two-step picker

- **WHEN** operator adds graded two-dimension product to cart
- **THEN** UI prompts dimension 1 then dimension 2 (or shows matrix) before confirming line

#### Scenario: Insufficient variant stock

- **WHEN** operator selects combination with zero or insufficient variant stock per RN-045
- **THEN** confirmation fails with validation error showing combination label
