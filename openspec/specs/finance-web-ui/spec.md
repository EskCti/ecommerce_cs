## ADDED Requirements

### Requirement: Receivables management UI

The Vue app SHALL provide receivables list, create/edit forms, settle action, and attachments per RF-070.

#### Scenario: Settle receivable from UI

- **WHEN** user clicks settle on open receivable row
- **THEN** UI calls settle API and updates row status

### Requirement: Payables and purchases UI

The Vue app SHALL provide payables CRUD, purchases list, and settle actions per RF-071/072.

#### Scenario: Purchases list page

- **WHEN** user navigates to finance purchases
- **THEN** DataTable shows purchase payables filtered by type

### Requirement: Commissions and cash flow UI

The Vue app SHALL provide commissions list with pay action and cash flow summary view per RF-074/075.

#### Scenario: Cash flow dashboard

- **WHEN** user selects date range on cash flow page
- **THEN** chart or table shows consolidated inflows and outflows

### Requirement: Permission guards

Finance routes SHALL require RBAC permissions aligned with legacy finance menus.

#### Scenario: Unauthorized finance access

- **WHEN** user without finance permission opens finance route
- **THEN** router blocks access
