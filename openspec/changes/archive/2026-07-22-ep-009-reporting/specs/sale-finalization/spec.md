## MODIFIED Requirements

### Requirement: Sale finalization with receipt

Upon successful sale finalization, the system SHALL complete sale persistence, emit SaleCompleted event, and expose receipt generation capability via Reporting GenerateReceiptQuery per RF-057.

#### Scenario: Receipt available after finalize

- **WHEN** FinalizeSaleUseCase completes successfully
- **THEN** client can request GET /api/reporting/receipts/{saleId} and receive valid receipt PDF

#### Scenario: Receipt not available before finalize

- **WHEN** client requests receipt for pending cart SaleId
- **THEN** response status is 404 or 409
