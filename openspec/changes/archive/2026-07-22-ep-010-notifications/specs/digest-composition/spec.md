## ADDED Requirements

### Requirement: Compose digest from query ports

DailyDigestComposer SHALL aggregate read-only data from Finance, Catalog, and Platform query ports without mutating source aggregates.

#### Scenario: Receivables due today included

- **WHEN** Finance port returns receivables due today
- **THEN** DigestContent includes receivable count and total amount

#### Scenario: Low stock summary included

- **WHEN** Catalog port returns low stock products
- **THEN** DigestContent includes product count and top product names

#### Scenario: Billing alert included

- **WHEN** Platform port returns pending tenant invoice
- **THEN** DigestContent includes billing alert text

### Requirement: Message template formatting

Composer SHALL format DigestContent into MessageTemplate suitable for WhatsApp text limits.

#### Scenario: Template within length

- **WHEN** digest is composed
- **THEN** rendered message does not exceed WhatsApp provider maximum length without truncation marker
