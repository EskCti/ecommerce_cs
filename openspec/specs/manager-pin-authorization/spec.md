# manager-pin-authorization Specification

## Purpose

Manager PIN verification for cash register operations, separate from login password and stored hashed.
## Requirements
### Requirement: Manager PIN verification endpoint

The system SHALL expose `POST /api/auth/verify-manager-pin` to validate a manager PIN for cash register operations.

#### Scenario: Valid manager PIN

- **WHEN** authenticated user submits correct manager PIN for an authorized Gerente or Administrador
- **THEN** response status is 200 confirming authorization

#### Scenario: Invalid manager PIN

- **WHEN** user submits incorrect manager PIN
- **THEN** response status is 401 or 403 without revealing whether user exists

### Requirement: Manager PIN stored hashed

The system SHALL store manager PINs using bcrypt (or equivalent secure hash), never plaintext.

#### Scenario: PIN persistence

- **WHEN** manager PIN is saved or migrated from legacy
- **THEN** only hashed value is stored in database

### Requirement: Separation from login password

Manager PIN SHALL be modeled separately from user login password (`ManagerPin` VO distinct from `PasswordHash`).

#### Scenario: Login password does not authorize caixa

- **WHEN** user provides correct login password but not manager PIN
- **THEN** verify-manager-pin endpoint rejects the request

### Requirement: Sales cash session manager authorization

Sales BC SHALL require successful manager PIN verification before OpenCashSession and CloseCashSession commands are accepted.

#### Scenario: Valid manager PIN for cash open

- **WHEN** Sales open session request includes verified manager PIN token or verification step succeeded
- **THEN** CashSession open proceeds

#### Scenario: Close session without PIN rejected

- **WHEN** close cash session is requested without prior manager PIN verification
- **THEN** Sales returns 403 referencing manager authorization required

