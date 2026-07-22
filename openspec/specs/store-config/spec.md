# store-config Specification

## Purpose

Maintain one store configuration aggregate per tenant (name, contacts, CNPJ, address, discount, commission, report format, integration token), with create-on-first-read and tenant-scoped API access.

## Requirements

### Requirement: One store config per tenant

The system SHALL maintain exactly one StoreConfig aggregate per tenant.

#### Scenario: Load store config

- **WHEN** tenant administrator requests store configuration
- **THEN** system returns config for current tenant only

#### Scenario: Create on first access

- **WHEN** tenant has no config row in legacy or new store
- **THEN** system initializes default StoreConfig on first read or update

### Requirement: Store general settings

Tenant administrators SHALL update store name, contact info, CNPJ, and address via API.

#### Scenario: Update general info

- **WHEN** administrator submits valid store name, CNPJ, and address
- **THEN** StoreConfig persists updated values scoped to tenant

### Requirement: Discount and commission settings

The system SHALL persist default discount type (percentage or fixed) and default commission rate per tenant.

#### Scenario: Update discount settings

- **WHEN** administrator sets discount type to percentage and commission to 5
- **THEN** values are stored and returned on subsequent GET

### Requirement: Report and integration settings

The system SHALL persist report format (PDF/HTML), report logo path, and WhatsApp API token per tenant.

#### Scenario: Update report settings

- **WHEN** administrator sets report format to PDF and logo path
- **THEN** report settings are persisted for tenant

### Requirement: Store report format configuration

The system SHALL persist tenant report output format as PDF or HTML via ReportFormat value object. Reporting module SHALL read ReportFormat when rendering tenant reports and set HTTP content type accordingly.

#### Scenario: PDF format configured

- **WHEN** tenant ReportFormat is PDF and user exports any tenant report
- **THEN** response Content-Type is application/pdf

#### Scenario: HTML format configured

- **WHEN** tenant ReportFormat is HTML and user exports any tenant report
- **THEN** response Content-Type is text/html

### Requirement: WhatsApp credentials configuration

Store configuration SHALL persist tenant WhatsApp API token and system phone number. Notifications digest SHALL resolve credentials with tenant token overriding global default token when tenant token is non-empty per RN-071.

#### Scenario: Tenant token overrides global

- **WHEN** tenant ApiToken is configured and global default token exists
- **THEN** digest uses tenant ApiToken for WhatsApp gateway

#### Scenario: Global token fallback

- **WHEN** tenant ApiToken is empty and global default token is configured
- **THEN** digest uses global default token

#### Scenario: System phone destination

- **WHEN** tenant system phone is configured
- **THEN** digest sends WhatsApp message to that phone number
