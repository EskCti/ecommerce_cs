## MODIFIED Requirements

### Requirement: Finalize sale with payment

Operators SHALL finalize non-empty cart with payment method, discounts, and change per RF-053. Upon successful finalize, Finance integration SHALL receive SaleCompleted event with amounts required for commission accrual and receivable consistency.

#### Scenario: Successful cash sale

- **WHEN** operator finalizes cart with valid payment and change greater than or equal to zero
- **THEN** Sale aggregate is created, stock is decremented, SaleCompleted event is published, and Finance handler processes commission and receivable sync

#### Scenario: Credit sale requires customer

- **WHEN** operator selects credit terms without customer
- **THEN** finalize fails requiring customer selection

#### Scenario: SaleCompleted triggers finance handler

- **WHEN** sale finalizes with commission amount
- **THEN** Finance SaleCompleted handler records Commission aggregate
