## MODIFIED Requirements

### Requirement: WhatsApp credentials configuration

Store configuration SHALL persist tenant WhatsApp API token and system phone number. Notifications digest SHALL resolve credentials with tenant token overriding global default token when tenant token is non-empty per RN-071.

#### Scenario: Tenant token overrides global

- **WHEN** tenant ApiToken is configured and global default token exists
- **THEN** digest uses tenant ApiToken for WhatsApp gateway

#### Scenario: Global token fallback

- **WHEN** tenant ApiToken is empty and global default token is configured
- **THEN** digest uses global default token

#### Scenario: System phone destination

- **WHEN** tenant system phone is configured
- **THEN** digest sends WhatsApp message to that phone number
