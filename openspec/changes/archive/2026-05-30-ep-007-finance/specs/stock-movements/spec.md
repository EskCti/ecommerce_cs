## MODIFIED Requirements

### Requirement: Stock purchase

The system SHALL process product purchase updating cost/stock and publishing ProductPurchased event per RF-042. Finance BC SHALL consume ProductPurchased to create PurchasePayable with correct PaymentStatus per RN-061.

#### Scenario: Purchase updates inventory

- **WHEN** operator completes stock purchase with quantity and cost
- **THEN** stock and cost price update, ProductPurchased event is published, and Finance handler creates purchase payable

#### Scenario: Future due purchase payable open

- **WHEN** purchase event includes due date after today
- **THEN** Finance creates payable with PaymentStatus Open per RN-061
