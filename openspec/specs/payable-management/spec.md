## ADDED Requirements

### Requirement: Payable CRUD

Finance users SHALL manage payables for expenses and purchases per RF-071.

#### Scenario: Create expense payable

- **WHEN** user creates payable with type Expense, amount, and due date
- **THEN** payable is persisted for tenant

#### Scenario: Create purchase payable from catalog event

- **WHEN** ProductPurchased event is received with future due date per RN-061
- **THEN** PurchasePayable is created with PaymentStatus Open

### Requirement: Settle payable

Finance users SHALL settle payables (baixar) per RF-071.

#### Scenario: Settle payable

- **WHEN** user settles open payable
- **THEN** PaymentStatus becomes Settled via PayableSettlementService

### Requirement: Purchase listing

The system SHALL list purchase payables filtered by type Compra per RF-072.

#### Scenario: List purchases

- **WHEN** user requests purchases list
- **THEN** response returns payables with AccountType Purchase paginated

### Requirement: Recurring expense

Payables MAY include recurrence interval from frequency reference per RF-071.

#### Scenario: Create recurring expense

- **WHEN** user creates expense with recurrence days
- **THEN** payable stores Recurrence VO for scheduled regeneration (MVP: metadata only)

### Requirement: Payable attachments

Users SHALL attach file metadata to payables per RF-071.

#### Scenario: Add payable attachment

- **WHEN** user adds attachment to payable
- **THEN** attachment metadata is persisted on Payable aggregate
