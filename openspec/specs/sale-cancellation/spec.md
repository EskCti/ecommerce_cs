## ADDED Requirements

### Requirement: Cancel completed sale

Authorized managers SHALL cancel completed sales restoring inventory per RF-073 and RN-060.

#### Scenario: Cancel sale success

- **WHEN** manager cancels valid sale
- **THEN** sale is marked cancelled, linked receivable and commissions are removed via ACL, and stock is restored

#### Scenario: Unauthorized cancel rejected

- **WHEN** user without cancel permission attempts cancel
- **THEN** response status is 403

### Requirement: Sale cancellation event

Cancelled sales SHALL publish SaleCancelled domain event for downstream handlers.

#### Scenario: Stock restored on cancel

- **WHEN** SaleCancelled event is handled
- **THEN** Catalog restores stock quantities for all sale lines

### Requirement: List sales for management

The system SHALL list sales filtered by type Venda for tenant per RF-073.

#### Scenario: List tenant sales

- **WHEN** authorized user requests sales list with date filter
- **THEN** paginated sales with totals are returned for tenant
