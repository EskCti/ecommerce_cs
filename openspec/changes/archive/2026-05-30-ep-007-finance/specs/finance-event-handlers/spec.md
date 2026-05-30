## ADDED Requirements

### Requirement: SaleCompleted handler

Finance SHALL handle SaleCompleted integration event to ensure commission records and receivable projections are consistent with Sales finalize.

#### Scenario: Handler processes sale completed

- **WHEN** SaleCompleted event is published after PDV finalize
- **THEN** Finance handler creates or syncs Commission and ensures sale-linked receivable consistency idempotently

#### Scenario: Duplicate event ignored

- **WHEN** SaleCompleted event is replayed with same SaleId
- **THEN** handler completes without duplicate commission rows

### Requirement: ProductPurchased handler

Finance SHALL handle ProductPurchased event from Catalog to create purchase payable per RN-061.

#### Scenario: Purchase creates open payable

- **WHEN** ProductPurchased event has future due date
- **THEN** PurchasePayable is created with PaymentStatus Open

#### Scenario: Purchase with immediate payment

- **WHEN** ProductPurchased indicates immediate settlement
- **THEN** payable may be created already Settled per business rules

### Requirement: SaleCancelled coordination

When SaleCancelled occurs, Finance SHALL remove or reverse linked receivable and commission via port called from Sales cancel flow RN-060.

#### Scenario: Cancel sale removes finance records

- **WHEN** Sales CancelSale succeeds
- **THEN** linked receivable and commission records are removed via Finance port
