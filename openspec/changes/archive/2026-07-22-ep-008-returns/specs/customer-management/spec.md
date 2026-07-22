## MODIFIED Requirements

### Requirement: Find or create by CPF

The system SHALL provide an operation that returns existing customer by CPF or creates a new one atomically within tenant scope. Returns BC RegisterExchangeUseCase SHALL invoke this operation when exchange form provides CPF without existing CustomerId per RN-051.

#### Scenario: Existing CPF found

- **WHEN** exchange registration provides CPF already registered for tenant
- **THEN** existing customer is linked without duplicate row

#### Scenario: New CPF creates customer during exchange

- **WHEN** exchange registration provides new valid CPF and customer name
- **THEN** customer is created via FindOrCreateByCpf and linked to exchange
