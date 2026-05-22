## REMOVED Requirements

### Requirement: Legacy SAS DbContext read access

**Reason**: Historical data migrated to normalized schema; PHP decommissioned; ACL no longer needed.

**Migration**: ETL job copies data to EF normalized tables; repositories use RetailOps.*.Infrastructure only; delete LegacySasDbContext and related Fluent mappings after backup verification.

### Requirement: Dual-write legacy adapters

**Reason**: Parallel run complete; single source of truth is normalized schema.

**Migration**: Remove ISalesLegacyPort dual-write implementations; Sales writes through normalized repositories only after cutover gate passes.
