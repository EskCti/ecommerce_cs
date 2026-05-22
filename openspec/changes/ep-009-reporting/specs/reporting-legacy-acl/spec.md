## ADDED Requirements

### Requirement: Legacy report SQL adapter

Infrastructure SHALL provide read-only SQL adapter mapping legacy rel_sistema scripts to CQRS read models during parallel run.

#### Scenario: Read-only enforcement

- **WHEN** LegacyReportSqlAdapter executes
- **THEN** only SELECT statements are allowed

### Requirement: Legacy row mapping

LegacyReportMapper SHALL map legacy result rows to Reporting DTOs without exposing legacy column names to Application layer.

#### Scenario: Sales row mapped

- **WHEN** legacy sales query returns rows
- **THEN** mapper produces SalesReportRow DTOs with domain-friendly property names

### Requirement: Domain isolation

Reporting Application layer SHALL NOT reference legacy table or script names.

#### Scenario: Application project dependencies

- **WHEN** Reporting.Application is analyzed
- **THEN** no Infrastructure.Legacy namespace references exist
