## ADDED Requirements

### Requirement: Sale receipt generation

Operators SHALL generate sale receipt PDF after sale completion per RF-057.

#### Scenario: Receipt for completed sale

- **WHEN** operator requests receipt for valid completed SaleId
- **THEN** PDF includes store header, sale lines, payment method, totals, and change amount

#### Scenario: Receipt for non-existent sale

- **WHEN** operator requests receipt for unknown or other-tenant SaleId
- **THEN** response status is 404

### Requirement: Receipt store branding

Receipt PDF SHALL include tenant store name and logo from Store Settings when configured.

#### Scenario: Logo on receipt

- **WHEN** tenant has report logo configured
- **THEN** receipt PDF header displays logo image

### Requirement: PDV receipt action

PDV UI SHALL expose receipt download or print action after successful sale finalization.

#### Scenario: Post-sale receipt link

- **WHEN** sale finalizes successfully on PDV
- **THEN** operator can open receipt PDF via reporting receipt endpoint
