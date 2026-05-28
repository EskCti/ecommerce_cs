## ADDED Requirements

### Requirement: Mobile cash session open

The Android app SHALL support opening cash session with initial float and manager PIN per US-060.

#### Scenario: Open session on Android

- **WHEN** operator completes open session form on mobile
- **THEN** app stores session context and navigates to PDV scan screen

### Requirement: Mobile barcode scan cart

The Android app SHALL scan barcodes and build cart per US-061.

#### Scenario: Scan adds item

- **WHEN** operator scans product barcode
- **THEN** item appears in mobile cart with quantity and price

#### Scenario: Grade selection on mobile

- **WHEN** scanned product requires grade
- **THEN** app prompts grade selection before line is finalized

### Requirement: Mobile checkout finalize

The Android app SHALL complete sale finalization flow per US-062.

#### Scenario: Finalize sale on Android

- **WHEN** operator completes mobile checkout with valid payment
- **THEN** app calls finalize API and shows success state
