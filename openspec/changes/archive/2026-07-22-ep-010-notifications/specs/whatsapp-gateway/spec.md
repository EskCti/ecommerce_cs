## ADDED Requirements

### Requirement: WhatsApp gateway port

Notifications application SHALL send messages through IWhatsAppGatewayPort abstraction.

#### Scenario: Successful send

- **WHEN** handler calls SendTextAsync with valid credentials and phone
- **THEN** gateway returns success Result

#### Scenario: Provider failure

- **WHEN** external API returns error
- **THEN** gateway returns failure Result without throwing unhandled exception

### Requirement: Enviame adapter

Infrastructure SHALL implement IWhatsAppGatewayPort using api.enviame.com.br HTTP API replacing legacy file_get_contents.

#### Scenario: HTTP request formed correctly

- **WHEN** adapter sends message
- **THEN** HTTP request includes tenant or global token per RN-071 and destination phone number

### Requirement: Domain isolation

Notifications Core and Application layers SHALL NOT reference enviame URL or HTTP client types.

#### Scenario: Core project dependencies

- **WHEN** Notifications.Core is analyzed
- **THEN** no HttpClient or external API URL references exist
