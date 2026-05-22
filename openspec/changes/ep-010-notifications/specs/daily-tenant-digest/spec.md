## ADDED Requirements

### Requirement: Send daily tenant digest

The system SHALL send a daily WhatsApp digest summary to each active tenant per RF-090.

#### Scenario: Digest sent successfully

- **WHEN** daily job runs for tenant with valid phone and credentials
- **THEN** WhatsApp message is delivered containing digest summary sections

#### Scenario: Skip tenant without phone

- **WHEN** tenant has no system phone configured
- **THEN** digest is skipped and no WhatsApp call is made

#### Scenario: Skip tenant without token

- **WHEN** tenant and global WhatsApp tokens are both empty
- **THEN** digest is skipped and failure is logged

### Requirement: Digest content sections

Daily digest message SHALL include receivables due today, low stock summary, and tenant billing alert when applicable per RF-090.

#### Scenario: Full digest content

- **WHEN** tenant has due receivables, low stock products, and pending billing
- **THEN** message includes all three sections with counts and key details

#### Scenario: Partial digest content

- **WHEN** tenant has only low stock alerts and no due receivables
- **THEN** message includes low stock section and omits empty sections

### Requirement: Digest sent event

Successful digest delivery SHALL publish DigestSent domain event.

#### Scenario: Event after send

- **WHEN** WhatsApp gateway returns success
- **THEN** DigestSent event is published with tenant id and digest date
