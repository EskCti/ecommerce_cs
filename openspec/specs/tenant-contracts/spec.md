# tenant-contracts Specification

## Purpose

Software rental contracts associated with tenant companies, compatible with legacy `contratos` persistence.

## Requirements

### Requirement: Contract per company

SAS operators SHALL save and retrieve software rental contracts associated with a company.

#### Scenario: Save contract

- **WHEN** SAS operator saves contract HTML/text and date for a company
- **THEN** contract is persisted as child of Company aggregate

#### Scenario: List company contracts

- **WHEN** SAS operator requests contracts for a company id
- **THEN** response lists all contracts ordered by date descending

### Requirement: Contract template support

The system SHALL support contract body content compatible with legacy `contratos.texto` field.

#### Scenario: Contract content retrieved

- **WHEN** contract is loaded from persistence
- **THEN** full text content is available for display or PDF export (future)
