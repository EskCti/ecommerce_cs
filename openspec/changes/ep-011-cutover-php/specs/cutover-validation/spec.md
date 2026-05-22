## ADDED Requirements

### Requirement: Pre-cutover gate checklist

Cutover SHALL NOT proceed until all EP-001 through EP-010 acceptance criteria and Sales parallel run thresholds are satisfied.

#### Scenario: Sales parallel run gate

- **WHEN** pre-cutover checklist runs
- **THEN** Sales parallel run divergence is at or below 0.1% for minimum 2 weeks

#### Scenario: All BC modules enabled

- **WHEN** pre-cutover checklist runs
- **THEN** Migration Modules flags show true for Auth through Notifications

### Requirement: Full E2E regression suite

Full integration test suite SHALL pass as mandatory gate before production cutover.

#### Scenario: E2E suite green

- **WHEN** cutover validation executes
- **THEN** all E2E tests across Auth, Catalog, Sales, Finance, Returns, Reporting, and Notifications pass

### Requirement: Zero PHP tenant flags

Production configuration SHALL have zero tenants flagged for PHP legacy routing at cutover completion per US-110.

#### Scenario: No legacy tenants

- **WHEN** cutover validation completes
- **THEN** migration status reports zero tenants on PHP path
