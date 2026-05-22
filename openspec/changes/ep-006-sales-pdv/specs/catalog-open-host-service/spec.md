## MODIFIED Requirements

### Requirement: Stock reservation port

The catalog SHALL provide reserve and release stock operations for Sales cart lifecycle. Reservations SHALL be tied to CashSessionId and line id so Sales BC can release on cart changes and commit on finalize.

#### Scenario: Reserve stock for cart line

- **WHEN** Sales requests reservation of quantity 2 for product with CashSessionId and line reference
- **THEN** available stock decreases by 2 until release or sale confirmation

#### Scenario: Commit reservation on finalize

- **WHEN** Sales finalizes sale successfully
- **THEN** reserved stock is converted to permanent stock decrement via Catalog port

#### Scenario: Release reservation

- **WHEN** Sales releases reservation on cart item removal or session cancel
- **THEN** available stock is restored
