## ADDED Requirements

### Requirement: Self-service trial registration

The system SHALL expose a public endpoint to register a trial tenant with an administrator user.

#### Scenario: Successful trial registration

- **WHEN** prospect submits valid company and admin data with unique email
- **THEN** system creates Company with trial flag, sets payment due date to today plus configured trial days, and creates admin user

#### Scenario: Duplicate email rejected

- **WHEN** prospect submits email already registered
- **THEN** response status is 409 with message indicating duplicate email

### Requirement: Trial expiration policy

The system SHALL deactivate trial companies and their users when trial due date is before today per RN-003.

#### Scenario: Expired trial company

- **WHEN** company has trial flag and due date before today
- **THEN** company and associated users are marked inactive

### Requirement: Platform trial configuration

The system SHALL read global trial period (`dias_teste`) from platform configuration (legacy `config` empresa=0).

#### Scenario: Trial period applied

- **WHEN** trial is registered with platform config dias_teste equal to 7
- **THEN** company due date is set to registration date plus 7 days
