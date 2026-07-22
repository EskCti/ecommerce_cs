## ADDED Requirements

### Requirement: Profit statement export

Authorized managers SHALL export simplified profit statement (revenues minus costs and expenses) per RF-080.

#### Scenario: Profit statement for period

- **WHEN** manager requests profit report with valid date range
- **THEN** PDF shows revenue total, cost total, expense total, and net profit

#### Scenario: Empty period

- **WHEN** date range has no financial movements
- **THEN** PDF is generated with zero totals and explicit empty-state message

### Requirement: Profit data sources

Profit statement SHALL aggregate settled receivables, purchase costs, and payables from Finance read models without mutating Finance aggregates.

#### Scenario: Finance read-only consumption

- **WHEN** profit query executes
- **THEN** no Finance write operations occur
