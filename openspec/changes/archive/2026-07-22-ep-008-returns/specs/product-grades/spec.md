## MODIFIED Requirements

### Requirement: Grade stock adjustment

When stock movement affects graded product, corresponding GradeOption stock and GradeMovementDetail are updated per RN-031. Returns exchange SHALL record GradeMovementDetail with types Troca Entrada and Troca Saída for inbound and outbound products.

#### Scenario: Troca entrada grade detail

- **WHEN** exchange registers with graded inbound product
- **THEN** GradeMovementDetail Troca Entrada is recorded with quantity 1

#### Scenario: Troca saída grade detail

- **WHEN** exchange registers with graded outbound product
- **THEN** GradeMovementDetail Troca Saída is recorded with quantity 1
