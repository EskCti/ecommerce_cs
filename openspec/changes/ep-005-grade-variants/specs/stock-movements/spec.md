## ADDED Requirements

### Requirement: Graded stock movement by variant

Graded stock movements SHALL record GradeMovementDetail referencing GradeVariant (one or two legacy option ids) per RN-031.

#### Scenario: Manual entry on two-dimension variant

- **WHEN** operator records stock entry of 3 units for variant Azul + P
- **THEN** GradeVariant stock increases by 3 and GradeMovementDetail stores both option legacy ids

#### Scenario: Purchase with grade variant

- **WHEN** operator completes purchase including graded line with variant selection
- **THEN** purchase updates GradeVariant stock and creates detalhes_grade compatible movement

## MODIFIED Requirements

### Requirement: Stock purchase

The system SHALL process product purchase updating cost/stock and publishing ProductPurchased event per RF-042, including grade variant lines when applicable.

#### Scenario: Purchase updates inventory

- **WHEN** operator completes stock purchase with quantity and cost for non-graded product
- **THEN** stock and cost price update and ProductPurchased event is published for Finance integration

#### Scenario: Purchase updates graded variant

- **WHEN** operator completes purchase specifying GradeVariant and quantity
- **THEN** variant stock increases and ProductPurchased event includes variant reference
