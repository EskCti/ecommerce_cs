## ADDED Requirements

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
