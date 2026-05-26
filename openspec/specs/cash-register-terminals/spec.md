# cash-register-terminals Specification

## Purpose

Manage physical cash register terminals per tenant (legacy `caixas` cadastro), including operator assignment and registration status, exposed to Sales BC via stable CashRegisterTerminalId references.

## Requirements

### Requirement: Cash register terminal CRUD

Tenant administrators SHALL manage physical cash register terminals (legacy `caixas` cadastro).

#### Scenario: Register terminal

- **WHEN** administrator creates terminal with name
- **THEN** CashRegisterTerminal is persisted for tenant

#### Scenario: Assign operator

- **WHEN** administrator assigns valid operator user to terminal
- **THEN** terminal stores AssignedOperatorId referencing Identity user

#### Scenario: List terminals

- **WHEN** administrator requests terminal list
- **THEN** response returns all terminals for current tenant with status and operator

### Requirement: Terminal status on registration

The system SHALL track terminal registration status (Open/Closed) compatible with legacy `caixas.status`.

#### Scenario: Update terminal status

- **WHEN** administrator updates terminal status to Closed
- **THEN** persisted status reflects Closed without opening PDV cash session

### Requirement: Tenant isolation

Cash register queries and mutations SHALL be scoped to current tenant.

#### Scenario: Cross-tenant access denied

- **WHEN** user attempts to access another tenant terminal by id
- **THEN** response status is 404 or 403
