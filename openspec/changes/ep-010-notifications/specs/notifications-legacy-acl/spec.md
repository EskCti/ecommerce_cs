## ADDED Requirements

### Requirement: Legacy digest query adapter

Infrastructure SHALL provide read-only adapter implementing digest query ports against legacy database during parallel run.

#### Scenario: Legacy receivables due query

- **WHEN** LegacyDigestQueryAdapter queries receivables due today
- **THEN** results match legacy mensagem.php SQL semantics for tenant

#### Scenario: Legacy low stock query

- **WHEN** LegacyDigestQueryAdapter queries low stock summary
- **THEN** results match legacy estoque baixo count semantics

### Requirement: Read-only enforcement

Legacy digest adapter SHALL execute only SELECT statements.

#### Scenario: No writes via ACL

- **WHEN** LegacyDigestQueryAdapter executes
- **THEN** no INSERT UPDATE or DELETE statements are issued

### Requirement: Migration to normalized queries

Application ports SHALL remain stable when adapter switches from legacy SQL to EF queries over normalized schema.

#### Scenario: Port contract unchanged

- **WHEN** normalized query implementation replaces legacy adapter
- **THEN** SendDailyTenantDigestHandler requires no code changes
