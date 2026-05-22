## MODIFIED Requirements

### Requirement: Login with email or CPF

The system SHALL authenticate users via `POST /api/auth/login` accepting either email or CPF plus password. Trial self-service registration SHALL be orchestrated by the Platform BC: `POST /api/auth/register` (or alias) delegates to `RegisterTrialCompanyUseCase`, creating Company and admin user atomically.

#### Scenario: Successful login returns JWT

- **WHEN** user submits valid email and password for an active user with required permissions
- **THEN** response status is 200 and body contains a JWT access token

#### Scenario: Register creates trial tenant

- **WHEN** prospect calls register endpoint with valid trial payload
- **THEN** Platform creates trial Company and admin user and returns success without requiring SAS operator

#### Scenario: Trial expired login blocked with message

- **WHEN** tenant trial or suspension policy marks company inactive
- **THEN** login returns 403 with message from platform block configuration when applicable
