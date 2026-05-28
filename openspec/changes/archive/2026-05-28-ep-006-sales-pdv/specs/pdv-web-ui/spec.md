## ADDED Requirements

### Requirement: PDV operator workspace

The Vue app SHALL provide fullscreen PDV workspace for scan, cart, payment, and finalize.

#### Scenario: PDV loads with open session

- **WHEN** operator opens PDV with active cash session
- **THEN** cart panel and barcode input are ready

#### Scenario: Open session flow in UI

- **WHEN** operator has no session
- **THEN** UI prompts open session with initial float and manager PIN step

### Requirement: Checkout and payment UI

The Vue app SHALL support payment method selection, discount, customer for credit, and change display.

#### Scenario: Finalize from Vue PDV

- **WHEN** operator completes checkout form with valid data
- **THEN** sale finalizes and UI shows receipt summary

### Requirement: Cash management UI

Managers SHALL perform withdrawal and close session from Vue PDV manager panel.

#### Scenario: Close session from UI

- **WHEN** manager enters counted cash and confirms close
- **THEN** session closes and breakage is displayed
