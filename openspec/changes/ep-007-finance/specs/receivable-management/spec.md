## ADDED Requirements

### Requirement: Manual receivable CRUD

Finance users SHALL create, read, update, and delete manual receivable accounts per RF-070.

#### Scenario: Create manual receivable

- **WHEN** user creates receivable with amount, due date, and description
- **THEN** receivable is persisted with PaymentStatus Open for tenant

#### Scenario: List receivables with filters

- **WHEN** user requests receivables list with date and status filters
- **THEN** paginated receivables for tenant are returned excluding Platform-only types handled elsewhere

### Requirement: Settle receivable

Finance users SHALL settle (baixar) open receivable accounts per RF-070.

#### Scenario: Settle open receivable

- **WHEN** user settles receivable with settlement date
- **THEN** PaymentStatus becomes Settled and settlement date is recorded

#### Scenario: Settle already settled rejected

- **WHEN** user attempts to settle already settled receivable
- **THEN** operation fails with validation error

### Requirement: Receivable attachments

Users SHALL attach file metadata to receivable accounts per RF-070.

#### Scenario: Add attachment metadata

- **WHEN** user adds attachment name and path to receivable
- **THEN** attachment is stored as child of Receivable aggregate
