## ADDED Requirements

### Requirement: Historical data migration job

The system SHALL provide an idempotent batch job that copies historical records from legacy tables to normalized EF Core schema per bounded context.

#### Scenario: Idempotent re-run

- **WHEN** HistoricalDataMigrationJob runs twice for same tenant and BC
- **THEN** second run produces no duplicate records and updates checkpoint only

#### Scenario: Migration order respected

- **WHEN** job executes for a tenant
- **THEN** BCs migrate in dependency order Platform through Returns

### Requirement: Reconciliation report

ETL job SHALL produce reconciliation report comparing row counts and sample checksums between legacy and normalized tables.

#### Scenario: Reconciliation pass

- **WHEN** ETL completes for tenant
- **THEN** report shows zero critical discrepancies for Sales, Catalog, and Finance aggregates

#### Scenario: Reconciliation failure blocks cutover

- **WHEN** reconciliation report has critical discrepancies
- **THEN** cutover gate remains blocked until resolved

### Requirement: Migration checkpoint

The system SHALL persist MigrationCheckpoint per tenant and bounded context tracking last successful ETL batch.

#### Scenario: Checkpoint updated on success

- **WHEN** ETL batch completes successfully for tenant BC
- **THEN** MigrationCheckpoint records completion timestamp
