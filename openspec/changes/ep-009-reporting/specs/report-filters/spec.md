## ADDED Requirements

### Requirement: Date range filter

Report endpoints SHALL accept start and end date filters per RF-082.

#### Scenario: Valid date range

- **WHEN** user provides from date less than or equal to to date
- **THEN** report query applies inclusive date filter

#### Scenario: Invalid date range rejected

- **WHEN** user provides from date after to date
- **THEN** response status is 400 with validation message

### Requirement: Status filter

Applicable reports SHALL accept payment or settlement status filter per RF-082.

#### Scenario: Status filter applied

- **WHEN** user selects open-only status on receivable-related report
- **THEN** results exclude settled records

### Requirement: Customer and seller filters

Sales-related reports SHALL accept optional customer and seller filters per RF-082.

#### Scenario: Customer filter

- **WHEN** user selects customer on sales report
- **THEN** results include only sales for that customer

#### Scenario: Seller filter

- **WHEN** user selects seller on sales report
- **THEN** results include only sales for that seller
