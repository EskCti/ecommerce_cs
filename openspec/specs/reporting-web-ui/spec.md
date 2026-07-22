## ADDED Requirements

### Requirement: Reports hub page

The Vue app SHALL provide reports hub listing available operational and financial reports per RF-080.

#### Scenario: Navigate to reports

- **WHEN** authorized user opens /reports
- **THEN** hub displays cards for sales, low stock, cash sessions, profit, and receipt

### Requirement: Filter modal

Report pages SHALL provide filter modal with date range, customer, seller, and status per RF-082.

#### Scenario: Apply filters and download

- **WHEN** user sets filters and clicks export
- **THEN** UI calls reporting API and triggers PDF download

### Requirement: Permission-guarded routes

Reports routes SHALL require RBAC permissions aligned with legacy Relatórios menu group.

#### Scenario: Unauthorized access blocked

- **WHEN** user without reports permission opens /reports
- **THEN** router redirects to unauthorized view

### Requirement: PDV receipt button

PDV sale completion view SHALL include receipt print/download action per RF-057.

#### Scenario: Print receipt from PDV

- **WHEN** operator clicks print receipt after sale
- **THEN** UI opens receipt PDF from reporting API
