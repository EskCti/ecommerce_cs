## ADDED Requirements

### Requirement: Legacy exchange mapping

Infrastructure SHALL map Exchange aggregate to legacy trocas table and related grade movement details.

#### Scenario: Persist exchange to legacy

- **WHEN** exchange is registered
- **THEN** LegacyExchangeAdapter writes trocas row and two detalhes_grade records via ACL

### Requirement: Stock adjustment delegation

Legacy exchange adapter SHALL delegate stock changes to IStockLegacyPort without domain referencing legacy tables.

#### Scenario: Stock port called on register

- **WHEN** exchange registers
- **THEN** IStockLegacyPort increases inbound and decreases outbound by 1 within same transaction as trocas insert

### Requirement: Domain isolation

Returns domain layer SHALL NOT reference legacy table or column names.

#### Scenario: Core project dependencies

- **WHEN** Returns.Core is analyzed
- **THEN** no Infrastructure.Legacy references exist
