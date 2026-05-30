## ADDED Requirements

### Requirement: Cash flow consolidation

The system SHALL provide consolidated cash flow for a date range per RF-074.

#### Scenario: Cash flow query

- **WHEN** user requests cash flow from date A to date B
- **THEN** response consolidates settled receivables and payables into inflow/outflow summary

#### Scenario: Empty period

- **WHEN** no movements exist in period
- **THEN** response returns zero totals without error

### Requirement: Cash flow read-only

Cash flow endpoint SHALL not mutate financial aggregates.

#### Scenario: Query does not settle accounts

- **WHEN** user runs cash flow query
- **THEN** no receivable or payable settlement occurs
