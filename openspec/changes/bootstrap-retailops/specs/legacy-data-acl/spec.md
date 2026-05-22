## ADDED Requirements

### Requirement: Legacy DbContext isolated

The infrastructure layer SHALL include `LegacySasDbContext` (or equivalent) mapping legacy `sas` schema tables for read access only.

#### Scenario: No domain reference to legacy context

- **WHEN** domain projects are inspected for references
- **THEN** no domain or application project references `LegacySasDbContext` directly

### Requirement: Legacy table mapping

The ACL SHALL map core legacy tables referenced in `docs/discovery/ecommerce-legado-php/domain-model.md` (e.g., `empresa`, `usuarios`, `produtos`) without renaming domain concepts in Core.

#### Scenario: Legacy query executes

- **WHEN** an infrastructure adapter queries `LegacySasDbContext` against a database loaded from `sas.sql`
- **THEN** mapped entities return data without raw SQL in application layer

### Requirement: Mapper boundary

Adapters SHALL translate legacy persistence models to domain DTOs or primitives via explicit mapper classes.

#### Scenario: Mapper converts legacy user row

- **WHEN** legacy user row is loaded from `usuarios`
- **THEN** mapper produces a domain-safe structure without exposing column names outside infrastructure
