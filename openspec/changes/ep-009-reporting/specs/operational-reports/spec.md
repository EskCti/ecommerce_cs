## ADDED Requirements

### Requirement: Sales report export

Authorized managers SHALL export sales report as PDF for a tenant within a date range per RF-080.

#### Scenario: Sales report with date filter

- **WHEN** manager requests sales report with valid from/to dates
- **THEN** system returns PDF containing sale rows with totals for the period

#### Scenario: Sales report with seller filter

- **WHEN** manager applies seller filter RF-082
- **THEN** PDF includes only sales attributed to selected seller

### Requirement: Low stock report export

Authorized managers SHALL export low stock report listing products below configured threshold per RF-080.

#### Scenario: Low stock PDF generated

- **WHEN** manager requests low stock report
- **THEN** system returns PDF with product name, current stock, and minimum level

### Requirement: Cash sessions report export

Authorized managers SHALL export cash session report for a date range per RF-080.

#### Scenario: Cash sessions PDF generated

- **WHEN** manager requests cash sessions report with valid date range
- **THEN** PDF lists sessions with opening, closing, sold amount, and variance

### Requirement: Tenant isolation

Operational report queries SHALL be scoped to current tenant.

#### Scenario: Cross-tenant data excluded

- **WHEN** tenant user requests any operational report
- **THEN** results contain only records for that tenant
