## ADDED Requirements

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
