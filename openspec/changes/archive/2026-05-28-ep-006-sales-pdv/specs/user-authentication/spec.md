## MODIFIED Requirements

### Requirement: Manager PIN verification endpoint

The system SHALL expose `POST /api/auth/verify-manager-pin` to validate a manager PIN for cash register operations. Sales BC SHALL require successful verification before OpenCashSession and CloseCashSession commands are accepted.

#### Scenario: Valid manager PIN for cash open

- **WHEN** Sales open session request includes verified manager PIN token or verification step succeeded
- **THEN** CashSession open proceeds

#### Scenario: Close session without PIN rejected

- **WHEN** close cash session is requested without prior manager PIN verification
- **THEN** Sales returns 403 referencing manager authorization required
