## ADDED Requirements

### Requirement: Finalize sale with payment

Operators SHALL finalize non-empty cart with payment method, discounts, and change per RF-053. Upon successful finalize, Finance integration SHALL receive SaleCompleted event with amounts required for commission accrual and receivable consistency.

#### Scenario: Successful cash sale

- **WHEN** operator finalizes cart with valid payment and change greater than or equal to zero
- **THEN** Sale aggregate is created, stock is decremented, SaleCompleted event is published, and Finance handler processes commission and receivable sync

#### Scenario: SaleCompleted triggers finance handler

- **WHEN** sale finalizes with commission amount
- **THEN** Finance SaleCompleted handler records Commission aggregate

#### Scenario: Empty cart rejected

- **WHEN** operator attempts finalize with empty cart per RN-044
- **THEN** operation fails with validation error

#### Scenario: Negative change rejected

- **WHEN** calculated change is negative per RN-043
- **THEN** finalize fails with validation error

### Requirement: Credit sale requires customer

Credit sales or future due date SHALL require CustomerId per RN-041.

#### Scenario: Credit sale without customer

- **WHEN** operator selects credit terms without customer
- **THEN** finalize fails requiring customer selection

#### Scenario: Cash sale marks paid

- **WHEN** sale is cash with due date today or past per RN-042
- **THEN** sale is marked paid with payment date today

### Requirement: Commission calculation

The system SHALL calculate seller commission per RN-046 on finalize.

#### Scenario: Commission with seller override

- **WHEN** seller has commission percent override
- **THEN** commission uses seller percent instead of store default

### Requirement: Parallel run accuracy

For pilot tenants during parallel run, finalized sale totals SHALL match legacy PHP within 0.1% tolerance over 2-week window.

#### Scenario: Parallel run within tolerance

- **WHEN** sale is finalized in parallel run mode
- **THEN** logger records C# total and legacy shadow total with difference within 0.1%

#### Scenario: Parallel run divergence alert

- **WHEN** logged difference exceeds 0.1%
- **THEN** alert is recorded for operations review
