# user-authentication Specification

## Purpose

Authenticate users via email or CPF with JWT, supporting legacy MD5 migration and access control at login time. Trial self-service registration is orchestrated by the Platform BC.

## Requirements

### Requirement: Login with email or CPF

The system SHALL authenticate users via `POST /api/auth/login` accepting either email or CPF plus password. Trial self-service registration SHALL be orchestrated by the Platform BC: `POST /api/auth/register` (or alias) delegates to `RegisterTrialCompanyUseCase`, creating Company and admin user atomically.

#### Scenario: Successful login returns JWT

- **WHEN** user submits valid email and password for an active user with required permissions
- **THEN** response status is 200 and body contains a JWT access token

#### Scenario: Successful login with CPF

- **WHEN** user submits valid CPF (without email) and correct password
- **THEN** response status is 200 and body contains a JWT access token

#### Scenario: Register creates trial tenant

- **WHEN** prospect calls register endpoint with valid trial payload
- **THEN** Platform creates trial Company and admin user and returns success without requiring SAS operator

#### Scenario: Trial expired login blocked with message

- **WHEN** tenant trial or suspension policy marks company inactive
- **THEN** login returns 403 with message from platform block configuration when applicable

### Requirement: Inactive or trial-expired user rejected

The system SHALL return HTTP 403 with an appropriate message when user is inactive or tenant trial has expired.

#### Scenario: Inactive user login

- **WHEN** user credentials are correct but account is inactive
- **THEN** response status is 403 with a message indicating inactive account

#### Scenario: Trial expired

- **WHEN** tenant trial period has expired per legacy business rules
- **THEN** response status is 403 with a message indicating expired trial

### Requirement: Non-privileged user without permissions rejected

The system SHALL reject login for non-Administrador/non-SAS users who have zero permission grants.

#### Scenario: Operator without grants

- **WHEN** user level is Operador and `usuarios_permissoes` has no rows for that user
- **THEN** response status is 403 with a message indicating missing permissions

### Requirement: Legacy MD5 password verification with bcrypt rehash

The system SHALL verify legacy MD5 passwords via ACL and re-hash to bcrypt on successful first login.

#### Scenario: Legacy MD5 login triggers rehash

- **WHEN** user password matches legacy MD5 hash and login succeeds
- **THEN** system persists bcrypt hash and subsequent login uses bcrypt verifier

### Requirement: Current user endpoint

The system SHALL expose `GET /api/auth/me` returning authenticated user profile and effective permissions.

#### Scenario: Authenticated me request

- **WHEN** client sends `GET /api/auth/me` with valid Bearer token
- **THEN** response includes user id, tenant id, level, and permission keys
