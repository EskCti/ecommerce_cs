## ADDED Requirements

### Requirement: List seller commissions

The system SHALL list commissions linked to sales per RF-075.

#### Scenario: List unpaid commissions

- **WHEN** finance user requests commissions with unpaid filter
- **THEN** paginated commission records for tenant are returned

#### Scenario: Seller views own commissions

- **WHEN** seller requests my commissions endpoint
- **THEN** only commissions for authenticated seller are returned

### Requirement: Pay commission

Finance users SHALL pay commissions individually or in batch, generating commission payment payable per RF-075.

#### Scenario: Pay single commission

- **WHEN** user pays commission
- **THEN** commission is marked paid and linked Payable tipo Pagamento is created via ACL

#### Scenario: Batch pay commissions

- **WHEN** user selects multiple unpaid commissions and confirms batch pay
- **THEN** all selected commissions are marked paid in single transaction

### Requirement: Commission accrual from sales

Commissions SHALL be recorded when sales complete via integration with Sales BC RN-046.

#### Scenario: Commission exists after sale

- **WHEN** SaleCompleted event includes commission amount
- **THEN** Commission aggregate is created or updated with unpaid status
